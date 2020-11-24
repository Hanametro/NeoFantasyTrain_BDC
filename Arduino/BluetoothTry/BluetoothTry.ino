
String cmd = "";
String COMMAND = "";
String checkcode = "";
void setup()
{
  Serial.begin(9600);
  pinMode(13, OUTPUT);
}


void loop()
{
  while (Serial.available() > 0)
  {
    cmd += char(Serial.read());
    delay(2);
    if (cmd.length() > 0)
    { COMMAND = cmd[0] + cmd[1] + cmd[2] + cmd[3];
      checkcode = cmd[4] + cmd[5];
      if (cmd == "HL01")
      {
        digitalWrite(13, HIGH);

      }
      if (cmd == "HL00")
      {
        digitalWrite(13, LOW);
      }
      Serial.println(checkcode);
      cmd = "";
    }
  }
}
