//Attention!This Program SHOULD BE USED Under AT mode!
void setup() {
  // put your setup code here, to run once:
  Serial.begin(38400);
 
}

void sendcmd()
{
    Serial.println("AT");
  while(Serial.available())
  {
    char ch;
    ch = Serial.read();
    Serial.print(ch);
  } // Get response: OK
  delay(1000); // wait for printing 

  
  Serial.println("AT+NAME=BR218-01");
  while(Serial.available())
  {
    char ch;
    ch = Serial.read();
    Serial.print(ch);
  }
  delay(1000);

  Serial.println("AT+ADDR?");
  while(Serial.available())
  {
    char ch;
    ch = Serial.read();
    Serial.print(ch);
  }
  delay(1000);

  Serial.println("AT+PSWD=0000");
  while(Serial.available())
  {
    char ch;
    ch = Serial.read();
    Serial.print(ch);
  }
  delay(1000);
  
}


void loop() {
    sendcmd();
}
