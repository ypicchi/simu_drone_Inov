import csv
import matplotlib.pyplot as plt
import numpy as np

hover_mode = (5, "hover")
altitude_mode = (6, "altitude")


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
        
        

    with open('pid_tunning_v2.txt', 'rt') as f:
        data = csv.reader(f, delimiter=';')
        for row in data:

            if(size==len(row)):
                
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
        fig.show()



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
        
        fig.show()





format_data()

display( altitude_mode )


