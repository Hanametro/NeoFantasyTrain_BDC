#include <SoftwareSerial.h>//头文件
SoftwareSerial  mySerial(7, 8); //定义7接收，8发送
//命令处理变量

byte cmdArr[]={0,0,0,0,0,0};//接收缓存
String COMMAND = "";//定义命令头
String level; //定义命令数值
String checkcode = "";//定义校验码
//模拟量测量变量
int voltagepin=A0;//定义电压测量口
int currentpin=A1;//定义电流测量口
float  vol;//定义测量电压
float  cur;//定义测量电流
String volstring;//定义测量电压反馈值
String curstring;//定义测量电流反馈值
//PWM控制变量
int    value=0;//定义占空比
int    acc=0;//定义PWM增或减[1加速 -1减速]
int    lim=0;//定义PWM目标值
int    delaytime=0;//定义加减速延时
unsigned long previousMillis = 0;//设置延时初始值
int limArray [] ={60,80,100,120,140,160,180,200,0,0,0,0,0,0,0,0,0};
int delayArray [] ={100,100,100,100,100,100,100,100,200,175,150,125,100,75,50,25,0};
byte sendArray[] ={0x7E,0x00,0x00,0x00,0x00,0x00,0x00,0xEF};
void bluetooth_Process()
{ 
  while ( mySerial.available() > 0){mySerial.readBytes(cmdArr,6);}//获取数据
  if(cmdArr[0]==0x7C &cmdArr[5]==0x7D)//处理验证通过的数据
  {
    byte LED_control=cmdArr[3];//bitRead格式 byte选择+位数选择，0为低电平
    digitalWrite(2, bitRead(LED_control,2));
    digitalWrite(3, bitRead(LED_control,7));
    digitalWrite(4, bitRead(LED_control,6));
    digitalWrite(12, bitRead(LED_control,5));
    digitalWrite(13, bitRead(LED_control,4)); 

    byte motor_control=cmdArr[2];
    digitalWrite(10, bitRead(motor_control,6));
    digitalWrite(11, bitRead(motor_control,7));
    level=String(cmdArr[2],HEX)[1];
    
    if (bitRead(motor_control,4)==1 ){lim=limArray[level.toInt()-1];delaytime=delayArray[level.toInt()-1];}
    else if(bitRead(motor_control,5)==1) {lim=limArray[level.toInt()+8];delaytime=delayArray[level.toInt()+8];}
    if(value<lim){acc=1;}
    if(value>lim){acc=-1;}
    
    bluetooth_Send();
    memset(cmdArr,0,sizeof(cmdArr));
  } 
  else{}//处理错误数据
}
void bluetooth_Send()//发送反馈
{ for(int i=0;i<8;i++)
  {
  mySerial.write(sendArray[i]);
  }  
}

void traction_calc()
{   int speed;  
    if(value<=40){speed=0;}
    else{speed=value-40;}
    float Force=50*level.toInt();//牵引力 KN
    switch(level.toInt())//非直线区域的拟合
    { case 6:
      if(speed>=87){Force=7200*3.6/speed;}break;
      case 7:
      if(60<speed<80){Force=420-1.2*speed;} else if(speed>=80){Force=7200*3.6/speed;}break;
      case 8:
      if(speed<5){Force=420;}else if(speed<80){Force=420-1.2*speed;} else{Force=7200*3.6/speed;}break;
      default:break;
    }
    float BForce=25*level.toInt();//再生制动力 KN
    float RForce=(1.20+0.0065*speed+0.000279*speed*speed);//空载阻力
    if(acc==1)
      {delaytime=1000/(3.6*(Force-RForce)/(100));}
    else if(acc==-1 & lim==0)
      {delaytime=1000/(3.6*(BForce+RForce)/(100));}
    else if(acc==-1 & lim>0) {delaytime=1000/(3.6*(RForce)/(100));}
}

void setup()
{
  Serial.begin(9600);// 电脑串口波特率
  mySerial.begin(9600);//蓝牙串口波特率
  pinMode(2, OUTPUT);//舱灯
  pinMode(3, OUTPUT);//1端头灯
  pinMode(4, OUTPUT);//2端头灯
  pinMode(9, OUTPUT); //电机
  pinMode(10, OUTPUT); //电机正转
  pinMode(11, OUTPUT); //电机反转
  pinMode(12, OUTPUT);//1端尾灯
  pinMode(13, OUTPUT);//2端尾灯
  
  digitalWrite(9, HIGH);
  analogWrite(9, value);
}

void loop()
{   
    //电压测定、编码
    vol=analogRead(voltagepin);
    vol=vol/1023*25;
    byte u1=floor(vol);
    byte u2=(100*(vol-u1));
    sendArray[2]=u1;
    sendArray[3]=u2;

    //电流测定、编码
    cur=analogRead(currentpin);
    cur=cur/1023*10;
    byte i1=floor(cur);
    byte i2=(100*(cur-i1));
    sendArray[4]=i1;
    sendArray[5]=i2;
    
    //蓝牙操作
    bluetooth_Process();
    //PWM加减速操作
    unsigned long currentMillis = millis();
    if (currentMillis - previousMillis >= delaytime) 
    {
      previousMillis = currentMillis;
      if(acc==-1&&value<=40){value=0;analogWrite(9, value);acc=0;}
      if(value!=lim){value=value+acc;}
      analogWrite(9, value);
      traction_calc(); 
    }
    sendArray[1]=(byte)(value);
    Serial.println(sendArray[1]);
}
