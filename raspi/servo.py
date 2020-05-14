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

# test servo!
for angle in range(5):
    kit.servo[0].angle = 0
    sleep(1)
    kit.servo[0].angle = 180
    sleep(1)
    
    