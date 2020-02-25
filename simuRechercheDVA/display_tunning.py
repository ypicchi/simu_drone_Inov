import csv
import matplotlib.pyplot as plt
import numpy as np


def format_data():
    
    fin = open("pid_tunning.txt", "rt")
    fout = open("pid_tunning_v2.txt", "wt")

    for line in fin:
            fout.write(line.replace(',', '.'))
            
    fin.close()
    fout.close() 



def display(size):
    time = [] # s
    objective = [] # speed
    vertical_speed = [] # speed
    thrust = [] # force
    altitude = [] # distance

    with open('pid_tunning_v2.txt', 'rt') as f:
        data = csv.reader(f, delimiter=';')
        for row in data:

            if(size==len(row)):
                
                time = time + [float(row[0])]
            
                objective = objective + [float(row[1])]
                vertical_speed = vertical_speed + [float(row[2])]
                thrust = thrust + [float(row[3])]
                altitude = altitude + [float(row[4])]
            
    objective = np.array(objective)
    vertical_speed = np.array(vertical_speed)

    """
    plt.xlabel('Time')
    plt.ylabel('Speed & Thrust')
    plt.plot(time, objective, 'r', time, vertical_speed, 'b', time, thrust, 'g--')
    
    plt.show()
    """
 

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


format_data()
display(5)


