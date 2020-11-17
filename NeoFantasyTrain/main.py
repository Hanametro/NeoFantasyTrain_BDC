# -*- coding: utf-8 -*-
import tkinter as tk
import threading
import time
import os
import serial
from PIL import Image,ImageTk

class Application():
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
        self.slide1 = tk.Scale(self.frame2, font=defaultFont,from_=-8, to=8, orient=tk.VERTICAL, width=50,length=630, showvalue=0,tickinterval=1, resolution=1, command=self.controlPWM)
        self.slide2 = tk.Scale(self.frame2, font=defaultFont,from_=1, to=-1, orient=tk.VERTICAL, width=50,length=180, showvalue=0,tickinterval=1, resolution=1, command=self.controlPWM)
        self.label4=tk.Label(self.frame2,text='机车\n方向', font=defaultFont)
        self.label5=tk.Label(self.frame2,text='手柄\n控制', font=defaultFont)
        self.label4.place(x=200,y=320,anchor='center')
        self.label5.place(x=110,y=320,anchor='center')
        self.slide1.place(x=50,y=320,anchor='center')
        self.slide2.place(x=250,y=320,anchor='center')
        self.frame2.place(x=970,y=20,anchor='nw')


        self.frame3=tk.LabelFrame(self.form,text='功能开关',font=defaultFont,height=200,width=640)
        self.slide3 = tk.Scale(self.frame3, font=defaultFont,from_=1, to=-1, orient=tk.VERTICAL, width=30,length=100, showvalue=0,tickinterval=1, resolution=1, command=self.headlight_Control)
        self.slide4 = tk.Scale(self.frame3, font=defaultFont,from_=-1, to=1, orient=tk.VERTICAL, width=30,length=100, showvalue=0,tickinterval=1, resolution=1, command=self.rearlight_Control)
        self.label6=tk.Label(self.frame3,text='灯光\n控制', font=defaultFont)
        self.label7=tk.Label(self.frame3,text='头灯', font=smallFont)
        self.label8=tk.Label(self.frame3,text='尾灯', font=smallFont)
        self.frame4=tk.LabelFrame(self.frame3,text='运行状态指示',font=defaultFont,height=150,width=300)
        self.slide3.place(x=50,y=100,anchor='center')
        self.slide4.place(x=130,y=100,anchor='center')
        self.label6.place(x=100,y=10,anchor='center')
        self.label7.place(x=45,y=30,anchor='nw')
        self.label8.place(x=125,y=30,anchor='nw')
        self.frame4.place(x=200,y=5,anchor='nw')
        self.frame3.place(x=640,y=600,anchor='center')


        self.frame4=tk.LabelFrame(self.form,text='蓝牙控制台',font=defaultFont,height=200,width=300)
        self.label3=tk.Label(self.frame4,text='蓝牙状态', font=defaultFont)
        self.labelB=tk.Label(self.frame4,text='连接端口', font=smallFont)
        self.labelC=tk.Label(self.frame4,text='命令视图', font=smallFont)
        self.richtext1=tk.Text(self.frame4,height=6,width=40,font=smallFont)
        self.COMtuple=['COM'+str(i) for i in range(0,31)]
        self.spinbox1=tk.Spinbox(self.frame4,font=defaultFont,values=self.COMtuple)
        self.button1=tk.Button(self.frame4,text='连接',width=10, font=smallFont,command=lambda:self.bluetoothCon(str(self.spinbox1.get())))
        self.button2=tk.Button(self.frame4,text='发送测试',width=10, font=smallFont,command=self.test)
        self.button2.place(x=250,y=10,anchor='center')
        self.label3.place(x=150,y=10,anchor='center')
        self.labelB.place(x=150,y=35,anchor='center')
        self.spinbox1.place(x=120,y=55,anchor='center')
        self.button1.place(x=250,y=55,anchor='center')
        self.labelC.place(x=150,y=80,anchor='center')
        self.richtext1.place(x=150,y=125,anchor='center')
        self.frame4.place(x=5,y=500,anchor='nw')

    def test(self):
        self.ser.write(("test "+time.strftime("%H:%M:%S", time.localtime())).encode())
        self.richtext1.insert(tk.INSERT,'TXD:已发送测试消息\n')

    def controlPWM(self):
        pass
    def headlight_Control(self):
        cmd='HL'
        return cmd
    def rearlight_Control(self):
        cmd='RL'
        return cmd


    def bluetoothCon(self,COMname='COM1',command='...'):
        def conn(self,COMname,command):
            self.richtext1.insert(tk.INSERT,'正在连接蓝牙串口:'+COMname+"...等待下位机操作\n")
            self.ser = serial.Serial(COMname, 9600,timeout=1)
            testmsg='AskForConnect'
            self.ser.write(testmsg.encode())
            self.richtext1.insert(tk.END,"蓝牙已连接！"+"\n")
            self.button1.config(text='已连接')
            while True:
                data = str(self.ser.read_all(),'utf-8')
                if data =='':
                    pass
                else:
                    self. richtext1.insert(tk.END,"RXD:"+data+"\n")

                if data=='REPLY':
                    confirmsg='TESTOK'
                    self.ser.write(confirmsg.encode())
                    self.richtext1.insert(tk.END,"TXD:"+confirmsg+"\n")
                time.sleep(0.1)
          
        t = threading.Thread(target=lambda:conn(self,COMname,command))
        t.start()



if __name__ == '__main__':
    form1 = tk.Tk()
    form1.title('Neo Fantasy Train')
    form1.geometry('1280x720') 
    LKJ=tk.PhotoImage(file='resources/LKJ.png')
    label0=tk.Label(form1,image=LKJ)
    label0.place(x=320,y=0,anchor='nw')



    appform=Application(form1)
    appform.setWindow()
    form1.mainloop()