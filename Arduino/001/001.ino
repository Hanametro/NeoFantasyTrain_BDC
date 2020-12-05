#include <SoftwareSerial.h>//头文件
SoftwareSerial  mySerial(7, 8); //定义7接收，8发送


String cmd = "";//定义接收
String COMMAND = "";//定义命令
String checkcode = "";//定义校验码

void setup()
{
  Serial.begin(9600);
  mySerial.begin(9600);//设置波特率
  pinMode(2, OUTPUT);//舱灯
  pinMode(3, OUTPUT);//1端头灯
  pinMode(4, OUTPUT);//2端头灯
  pinMode(12, OUTPUT);//1端尾灯
  pinMode(13, OUTPUT);//2端尾灯
}


void loop()
{
  while ( mySerial.available() > 0)//获取数据时
  {
    cmd += char( mySerial.read());//获取的值赋给cmd
    delay(10);
    if (cmd.length() == 6) //判断接收值是6位即正确
    {
      if (cmd == "T010C0")//连接测试
      {
        checkcode = "R01";
        mySerial.println(checkcode);
        break;
      }
      if (cmd[4] != 'C')//校验测试
      {
        checkcode = "error";
        mySerial.println(checkcode);
        break;
      }
      COMMAND = (String)cmd[0] + (String)cmd[1] + (String)cmd[2] + (String)cmd[3];
      //前四位赋给COMMAND
      checkcode = "R" + (String)cmd[5];
      //后两位赋给checkcode
      mySerial.println(checkcode);//返回校验码
      Serial.println(COMMAND);
      if (COMMAND == "HL01")//1端头灯亮
      {
        digitalWrite(3, HIGH);
      }
      if (COMMAND == "HL11")//2端头灯亮
      {
        digitalWrite(4, HIGH);
      }
      if (COMMAND == "HL00")//头灯灭
      {
        digitalWrite(3, LOW);
        digitalWrite(4, LOW);
      }
      if (COMMAND == "RL01")//1端尾灯亮
      {
        digitalWrite(12, HIGH);
      }
      if (COMMAND == "RL11")//2端尾灯亮
      {
        digitalWrite(13, HIGH);
      }
      if (COMMAND == "RL00")//尾灯灭
      {
        digitalWrite(12, LOW);
        digitalWrite(13, LOW);
      }
      if (COMMAND == "CL01")//舱灯亮
      {
        digitalWrite(2, HIGH);
      }
      if (COMMAND == "CL00")//尾灯灭
      {
        digitalWrite(2, LOW);
      }
      cmd = "";//cmd清零
    }

  }
}
