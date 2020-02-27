import csv
import matplotlib.pyplot as plt
import numpy as np

hover_mode = (5, "hover")
altitude_mode = (6, "altitude")
yaw_mode = (6, "yaw")
pitch_mode = (4, "pitch")

def format_data():
    fin = open("pid_tunning.txt", "rt")
    fout = open("pid_tunning_v2.txt", "wt")

    for line in fin:
            fout.write(line.replace(',', '.'))
            
    fin.close()
    fout.close() 



def display(mode):

    size , name_mod = mode
    time = [] # s

    if(mode == hover_mode):
        objective = []      # speed
        vertical_speed = [] # speed
        thrust = []         # force
        altitude = []       # distance


    elif(mode == altitude_mode):
        target_altitude = []
        actual_altitude = []
        target_speed = []
        actual_speed = []
        thrust = []

    elif(mode == yaw_mode):
        target_angle = []
        actual_angle = []
        target_angle_difference = []
        actual_angle_difference = []
        thrust = []
        
    elif(mode == pitch_mode):
        target_pitch = []
        actual_pitch = []
        thrust = []


        

    with open('pid_tunning_v2.txt', 'rt') as f:
        data = csv.reader(f, delimiter=';')
        
        for row in data:

            if(size==len(row) and len(row[len(row)-1]) > 0):
                
                time = time + [float(row[0])]

                if(mode == hover_mode):
                    objective = objective + [float(row[1])]
                    vertical_speed = vertical_speed + [float(row[2])]
                    thrust = thrust + [float(row[3])]
                    altitude = altitude + [float(row[4])]

                elif(mode == altitude_mode):
                    target_altitude = target_altitude + [float(row[1])]
                    actual_altitude = actual_altitude + [float(row[2])]
                    target_speed = target_speed + [float(row[3])]
                    actual_speed = actual_speed + [float(row[4])]
                    thrust = thrust + [float(row[5])]

                elif(mode == yaw_mode):
                    target_angle = target_angle + [float(row[1])]
                    actual_angle = actual_angle + [float(row[2])]
                    target_angle_difference = target_angle_difference + [float(row[3])]
                    actual_angle_difference = actual_angle_difference + [float(row[4])]
                    thrust = thrust + [float(row[5])]

                elif(mode == pitch_mode):
                    target_pitch = target_pitch + [float(row[1])]
                    actual_pitch = actual_pitch + [float(row[2])]
                    thrust = thrust + [float(row[3])]

            
    #objective = np.array(objective)
    #vertical_speed = np.array(vertical_speed)


    if(mode == hover_mode):
        fig, (ax1, ax2, ax3) = plt.subplots(3)
        fig.suptitle('PID tunning')
        
        ax1.plot(time, objective, 'r', time, vertical_speed, 'b')
        ax2.plot(time, thrust, 'g--')
        ax3.plot(time, altitude, 'm')

        fig.text(0.03, 0.75, 'Speed', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.5, 'Thrust', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.25, 'Altitude', ha='center', va='center', rotation='vertical')
        
        fig.text(0.5, 0.03, 'Time', ha='center', va='center')

        ax1.legend( ('objective', 'actual'))
      



    elif(mode == altitude_mode):
        fig, (ax1, ax2, ax3) = plt.subplots(3)
        fig.suptitle('PID tunning')
        
        ax1.plot(time, target_altitude, 'r', time, actual_altitude, 'b')
        ax2.plot(time, target_speed, 'm', time, actual_speed, 'k')
        ax3.plot(time, thrust, 'g--')
        
        fig.text(0.03, 0.75, 'Altitude', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.5, 'Speed', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.25, 'Thrust', ha='center', va='center', rotation='vertical')
        
        fig.text(0.5, 0.03, 'Time', ha='center', va='center')

        ax1.legend( ('objective', 'actual'))
        ax2.legend( ('objective', 'actual'))
        
       



    elif(mode == yaw_mode):
        fig, (ax1, ax2, ax3 ) = plt.subplots(3)
        fig.suptitle('PID tunning')
        
        ax1.plot(time, target_angle, 'r', time, actual_angle, 'b')
        ax2.plot(time, target_angle_difference, 'm', time, actual_angle_difference, 'k')
        ax3.plot(time, thrust, 'g--')

        fig.text(0.03, 0.75, 'Angle', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.5, 'Angle difference', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.25, 'Thrust', ha='center', va='center', rotation='vertical')
        
        fig.text(0.5, 0.03, 'Time', ha='center', va='center')

        ax1.legend( ('objective', 'actual'))
        ax2.legend( ('objective', 'actual'))
        
        
        
    elif(mode == pitch_mode):

        fig, (ax1, ax2) = plt.subplots(2)
        fig.suptitle('PID tunning')
        
        ax1.plot(time, target_pitch, 'r', time, actual_pitch, 'b')
        ax2.plot(time, thrust, 'g')
  
        fig.text(0.03, 0.66, 'Pitch', ha='center', va='center', rotation='vertical')
        fig.text(0.03, 0.33, 'Thrust', ha='center', va='center', rotation='vertical')
        
        fig.text(0.5, 0.03, 'Time', ha='center', va='center')

        ax1.legend( ('objective', 'actual'))

    fig.show()




format_data()

display( pitch_mode )


