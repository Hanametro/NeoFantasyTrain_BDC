#include <SoftwareSerial.h>//头文件
SoftwareSerial  mySerial(7, 8); //定义7接收，8发送

String cmd = "";//定义接收
String COMMAND = "";//定义命令头
String LEVEL = ""; //定义命令数值
String checkcode = "";//定义校验码
void pwmControl()
{
  return 0;
}

void currentMeasure()
{
  return 0;
}

void setup()
{
  Serial.begin(9600);// 电脑串口波特率
  mySerial.begin(9600);//蓝牙串口波特率
  pinMode(2, OUTPUT);//舱灯
  pinMode(3, OUTPUT);//1端头灯
  pinMode(4, OUTPUT);//2端头灯
  pinMode(5, OUTPUT); //1端电机PWM
  pinMode(6, OUTPUT); //2端电机PWM
  pinMode(10, OUTPUT); //电机正转
  pinMode(11, OUTPUT); //电机反转
  pinMode(12, OUTPUT);//1端尾灯
  pinMode(13, OUTPUT);//2端尾灯
}


void loop()
{
  while ( mySerial.available() > 0)//获取数据时
  {
    cmd += char( mySerial.read());//获取的值赋给cmd
    delay(10);
    Serial.println(cmd);

    if (cmd.length() == 6) //判断接收值是6位即正确
    { Serial.println(cmd);//电脑串口监视用
      if (cmd == "T010C0")//连接测试
      {
        checkcode = "R01";
        mySerial.println(checkcode);
      }
      if (cmd[4] != 'C')//校验测试
      {
        checkcode = "error";
        mySerial.println(checkcode);
      }
      COMMAND = (String)cmd[0] + (String)cmd[1] ;
      LEVEL = (String)cmd[2] + (String)cmd[3];
      //1,2位赋给COMMAND;3,4位给LEVEL
      checkcode = "R" + (String)cmd[5];
      //后两位赋给checkcode
      mySerial.println(checkcode);//返回校验码


      if (COMMAND == "HL")
      {
        if (LEVEL == "01") {
          digitalWrite(3, HIGH); //1端头灯亮
        }
        if (LEVEL == "11") {
          digitalWrite(4, HIGH); //2端头灯亮
        }
        if (LEVEL == "00") {
          digitalWrite(3, LOW);  //头灯灭
          digitalWrite(4, LOW);
        }
      }

      if (COMMAND == "RL")
      {
        if (LEVEL == "01") {
          digitalWrite(12, HIGH); //1端尾灯亮
        }
        if (LEVEL == "11") {
          digitalWrite(13, HIGH); //2端尾灯亮
        }
        if (LEVEL == "00") {
          digitalWrite(12, LOW);  //尾灯灭
          digitalWrite(13, LOW);
        }
      }

      if (COMMAND == "CL")
      { if (LEVEL == "11") {
          digitalWrite(2, HIGH); //舱灯亮
        }
        if (LEVEL == "00") {
          digitalWrite(2, LOW); //尾灯灭
        }

      }

      if (COMMAND == "EB") //紧急制动
      { digitalWrite(5, LOW);
        digitalWrite(6, LOW);
      }

      if (COMMAND == "DI") //换向
      { if (LEVEL == "01")
        {
          digitalWrite(10, HIGH);
          digitalWrite(11, LOW);
        }

        if (LEVEL == "11")
        {
          digitalWrite(10, LOW);
          digitalWrite(11, HIGH);
        }
        if (LEVEL == "00")
        {
          digitalWrite(10, LOW);
          digitalWrite(11, LOW);
        }
      }
      if (COMMAND == "PW")
      { if (LEVEL == "01") {

          digitalWrite(5, HIGH);
          digitalWrite(6, HIGH);
          analogWrite(5, 20);
          analogWrite(6, 20);
        }
        if (LEVEL == "02") {

          digitalWrite(5, HIGH);
          digitalWrite(6, HIGH);
          analogWrite(5, 50);
          analogWrite(6, 50);
        }
        if (LEVEL == "03") {

          digitalWrite(5, HIGH);
          digitalWrite(6, HIGH);
          analogWrite(5, 100);
          analogWrite(6, 100);
        }
        if (LEVEL == "04") {

          digitalWrite(5, HIGH);
          digitalWrite(6, HIGH);
          analogWrite(5, 160);
          analogWrite(6, 160);
        }
        if (LEVEL == "05") {

          digitalWrite(5, HIGH);
          digitalWrite(6, HIGH);
          analogWrite(5, 200);
          analogWrite(6, 200);
        }
        if (LEVEL == "06") {

          digitalWrite(5, HIGH);
          digitalWrite(6, HIGH);
          analogWrite(5, 255);
          analogWrite(6, 255);
        }
        if (LEVEL == "11") {

          digitalWrite(5, LOW);
          digitalWrite(6, LOW);

        }

      }
      cmd = "";//cmd清零
    }



  }

}
