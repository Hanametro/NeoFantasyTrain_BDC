#include <SoftwareSerial.h>

//初始一个软件串口 serial2(Rxd, Txd)

SoftwareSerial  mySerial(7,8);

void setup() {

  Serial.begin(9600);

  mySerial.begin(38400);//设置AT模式的串口波特率是38400

}

void loop() {

  if(Serial.available())

    mySerial.write(Serial.read());

  if(mySerial.available())

    Serial.write(mySerial.read());

}
