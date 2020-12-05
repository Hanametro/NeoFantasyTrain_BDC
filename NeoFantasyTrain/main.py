# -*- coding: utf-8 -*-
import tkinter as tk
import tkinter.ttk as ttk
import threading
import time
import os
import serial
import serial.tools.list_ports as ports 
import random
from PIL import Image,ImageTk

class mainApplication():
    #函数定义区
    def __init__(self,form):
        self.form=form
    #定义窗口
    def setWindow(self):
        defaultFont=('SimHei', 12)
        bigFont=('SimHei', 24)
        smallFont=('SimHei', 8)

        self.frame1=tk.LabelFrame(self.form,text='车辆运行参数',font=defaultFont,height=420,width=300)
        self.labelA=tk.Label(self.frame1,text='运行速度 （km/h）', font=defaultFont)
        self.label1=tk.Label(self.frame1,text='Ⅰ端电机电流 (A)', font=defaultFont)
        self.label2=tk.Label(self.frame1,text='Ⅱ端电机电流 (A)', font=defaultFont)
        self.textbox1=tk.Entry(self.frame1, font=bigFont,width=6)
        self.textbox2=tk.Entry(self.frame1, font=bigFont,width=6)
        self.textbox3=tk.Entry(self.frame1, font=bigFont,width=6)
        self.textbox1.insert(0,'0')
        self.textbox2.insert(0,'0')
        self.textbox3.insert(0,'0')
        self.labelA.grid(row=0,column=0)
        self.textbox3.grid(row=1,column=0)
        self.label1.grid(row=2,column=0)
        self.textbox1.grid(row=3,column=0)
        self.label2.grid(row=4,column=0)
        self.textbox2.grid(row=5,column=0)
        self.frame1.place(x=5,y=20,anchor='nw')

        self.frame2=tk.LabelFrame(self.form,text='司机控制器',font=defaultFont,height=680,width=300)
        self.slide1 = tk.Scale(self.frame2, font=defaultFont,from_=8, to=-8, orient=tk.VERTICAL, width=50,length=630, showvalue=0,tickinterval=1, resolution=1, command=self.controlPWM,state=tk.DISABLED)
        self.slide2 = tk.Scale(self.frame2, font=defaultFont,from_=1, to=-1, orient=tk.VERTICAL, width=50,length=180, showvalue=0,tickinterval=1, resolution=1, command=self.controlDirection,state=tk.DISABLED)
        self.label4=tk.Label(self.frame2,text='机车\n方向', font=defaultFont)
        self.label5=tk.Label(self.frame2,text='手柄\n控制', font=defaultFont)
        self.button3=tk.Button(self.frame2,text='紧急\n制动',width=5, font=defaultFont,command=self.EBreak,state=tk.DISABLED)
        self.label4.place(x=200,y=320,anchor='center')
        self.label5.place(x=110,y=320,anchor='center')
        self.button3.place(x=120,y=150,anchor='center')
        self.slide1.place(x=50,y=320,anchor='center')
        self.slide2.place(x=250,y=320,anchor='center')
        self.frame2.place(x=970,y=20,anchor='nw')


        self.frame3=tk.LabelFrame(self.form,text='功能开关',font=defaultFont,height=200,width=640)
        self.slide3 = tk.Scale(self.frame3, font=defaultFont,from_=1, to=-1, orient=tk.VERTICAL, width=30,length=100, showvalue=0,tickinterval=1, resolution=1, command=self.headlight_Control,state=tk.DISABLED)
        self.slide4 = tk.Scale(self.frame3, font=defaultFont,from_=-1, to=1, orient=tk.VERTICAL, width=30,length=100, showvalue=0,tickinterval=1, resolution=1, command=self.rearlight_Control,state=tk.DISABLED)
        self.slide5 = tk.Scale(self.frame3, font=defaultFont,from_=1, to=0, orient=tk.VERTICAL, width=30,length=100, showvalue=0,tickinterval=1, resolution=1, command=self.cablight_Control,state=tk.DISABLED)
        self.label6=tk.Label(self.frame3,text='灯光控制', font=defaultFont)
        self.label7=tk.Label(self.frame3,text='前照灯', font=smallFont)
        self.label8=tk.Label(self.frame3,text='后端标志灯', font=smallFont)
        self.label9=tk.Label(self.frame3,text='机械室灯', font=smallFont)
        self.frame4=tk.LabelFrame(self.frame3,text='运行状态指示',font=defaultFont,height=150,width=300)
        self.slide3.place(x=50,y=100,anchor='center')
        self.slide4.place(x=130,y=100,anchor='center')
        self.slide5.place(x=210,y=100,anchor='center')
        self.label6.place(x=140,y=10,anchor='center')
        self.label7.place(x=38,y=30,anchor='nw')
        self.label8.place(x=108,y=30,anchor='nw')
        self.label9.place(x=188,y=30,anchor='nw')
        self.frame4.place(x=300,y=5,anchor='nw')
        self.frame3.place(x=640,y=600,anchor='center')


        self.frame4=tk.LabelFrame(self.form,text='蓝牙控制台',font=defaultFont,height=200,width=300)
        self.label3=tk.Label(self.frame4,text='蓝牙状态', font=defaultFont)
        self.labelB=tk.Label(self.frame4,text='连接端口', font=smallFont)
        self.labelC=tk.Label(self.frame4,text='命令视图', font=smallFont)
        self.richtext1=tk.Text(self.frame4,height=6,width=40,font=smallFont)
        self.COMList = ['COM'+str(i) for i in range(0,16)]

        self.Combobox1=ttk.Combobox(self.frame4,font=defaultFont,values=self.COMList)
        self.button1=tk.Button(self.frame4,text='连接',width=10, font=smallFont,command=lambda:self.bluetoothCon(str(self.Combobox1.get())))
        self.button2=tk.Button(self.frame4,text='发送测试',width=10, font=smallFont,command=self.test,state=tk.DISABLED)
        self.button2.place(x=250,y=10,anchor='center')
        self.label3.place(x=150,y=10,anchor='center')
        self.labelB.place(x=150,y=35,anchor='center')
        self.Combobox1.place(x=120,y=55,anchor='center')
        self.button1.place(x=250,y=55,anchor='center')
        self.labelC.place(x=150,y=80,anchor='center')
        self.richtext1.place(x=150,y=125,anchor='center')
        self.frame4.place(x=5,y=500,anchor='nw')



    def bluetoothCon(self,COMname):
        def conn(self,COMname):
            self.richtext1.insert(tk.INSERT,'正在连接蓝牙串口:'+COMname+"...等待下位机操作\n")
            self.button1.config(text='断开',command=self.breakBluetooth)
            self.ser = serial.Serial(COMname, 9600,timeout=1)
            testmsg='T010C0'
            self.ser.write(testmsg.encode())
            self.richtext1.insert(tk.END,"蓝牙已连接！"+"\n")

            self.slide2.config(state=tk.NORMAL)
            self.button2.config(state=tk.NORMAL)
            self.button3.config(state=tk.NORMAL)
            self.slide3.config(state=tk.NORMAL)
            self.slide4.config(state=tk.NORMAL)
            self.slide5.config(state=tk.NORMAL)
            while True:
                try:
                    data = str(self.ser.read_all(),'utf-8')
                    if data =='':
                        pass
                    else:
                        self. richtext1.insert(tk.END,time.strftime("%H:%M:%S", time.localtime())+" RXD:"+data+"\n")
                        if data=='R01':
                            confirmsg='TESTOK'
                            self.richtext1.insert(tk.END,"TXD:"+confirmsg+"\n")
                        else:
                            self.RXDhandle(data)
                except:
                    break
                time.sleep(0.4)
        t = threading.Thread(target=lambda:conn(self,COMname))
        t.start()
    
    def breakBluetooth(self):
        try:
            self.ser.close()
        except:
            pass
        self.richtext1.insert(tk.END,"蓝牙连接已关闭！\n")
        self.button1.config(text='连接',command=lambda:self.bluetoothCon(str(self.Combobox1.get())))
        self.slide2.config(state=tk.DISABLED)
        self.button2.config(state=tk.DISABLED)
        self.button3.config(state=tk.DISABLED)
        self.slide3.config(state=tk.DISABLED)
        self.slide4.config(state=tk.DISABLED)
        self.slide5.config(state=tk.DISABLED)
    

    def test(self):
        self.cmd='T01000'
        self.ser.write((self.cmd+time.strftime("%H:%M:%S", time.localtime())).encode())
        self.bluetooth_Feedback(self.cmd)


    def controlDirection(self,value):
        if int(value)==0:
            self.slide1.config(state=tk.DISABLED)
        else:
            self.slide1.config(state=tk.NORMAL)
        cmdID=['11','00','01']
        self.cmd='DI'+cmdID[int(value)+1]+self.checkcodeGen()
        self.ser.write(self.cmd.encode())
        self.bluetooth_Feedback(self.cmd)

    def controlPWM(self,value):
        if int(value)!=0:
            self.slide2.config(state=tk.DISABLED)
        else:
            self.slide2.config(state=tk.NORMAL)
        cmdID=['18','17','16','15','14','13','12','11','00','01','02','03','04','05','06','07','08']
        self.cmd='PW'+cmdID[int(value)+8]+self.checkcodeGen()
        self.ser.write(self.cmd.encode())
        self.bluetooth_Feedback(self.cmd)
    def EBreak(self):
        self.cmd='EB00'+self.checkcodeGen()
        self.ser.write(self.cmd.encode())
        self.bluetooth_Feedback(self.cmd)
        self.slide1.config(showvalue=0)

    def headlight_Control(self,value):
        cmdID=['11','00','01']
        self.cmd='HL'+cmdID[int(value)+1]+self.checkcodeGen()
        self.ser.write(self.cmd.encode())
        self.bluetooth_Feedback(self.cmd)
    def rearlight_Control(self,value):
        cmdID=['11','00','01']
        self.cmd='RL'+cmdID[int(value)+1]+self.checkcodeGen()
        self.ser.write(self.cmd.encode())
        self.bluetooth_Feedback(self.cmd)
    def cablight_Control(self,value):
        cmdID=['00','01']
        self.cmd='CL'+cmdID[int(value)]+self.checkcodeGen()
        self.ser.write(self.cmd.encode())
        self.bluetooth_Feedback(self.cmd)

    def checkcodeGen(self):
        self.checkcode='C'+str(random.randint(1,9))
        return self.checkcode
    def bluetooth_Feedback(self,text):
        self.richtext1.insert(tk.INSERT,time.strftime("%H:%M:%S", time.localtime())+' TXD:'+text+"\n")
    

    def RXDhandle(self,data):
        if data[0:2]=='IA':
            currentA=str(data[2:8])
            self.textbox1.delete(0,tk.END)
            self.textbox1.insert(0,currentA)
        elif data[0:2]=='IB':
            currentB=str(data[2:8])
            self.textbox2.delete(0,tk.END)
            self.textbox2.insert(0,currentB)
        else:
            pass


if __name__ == '__main__':

    form1 = tk.Tk()
    form1.title('Neo Fantasy Train')
    form1.geometry('1280x720') 
    LKJ=tk.PhotoImage(file='resources/LKJ.png')
    label0=tk.Label(form1,image=LKJ)
    label0.place(x=320,y=0,anchor='nw')

    appform=mainApplication(form1)
    appform.setWindow()
    form1.mainloop()