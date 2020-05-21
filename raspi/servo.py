# Make sure to enable I2C on the raspberry pi
# https://learn.adafruit.com/adafruits-raspberry-pi-lesson-4-gpio-setup/configuring-i2c
# Then install I2C tools and lib
# sudo apt-get install python-smbus
# sudo apt-get install i2c-tools
# sudo pip3 install adafruit-circuitpython-servokit

from time import sleep
from adafruit_servokit import ServoKit

# Initialize the servo shield
kit = ServoKit(channels=16)

kit.servo[1].actuation_range = 200.5
kit.servo[1].set_pulse_width_range(600, 2485)
servo_0 = 8
servo_90 = 98
servo_180 = 188

# test servo!
for angle in range(50):
    kit.servo[1].angle = servo_0
    sleep(2)
    kit.servo[1].angle = servo_180
    sleep(2)
    
    