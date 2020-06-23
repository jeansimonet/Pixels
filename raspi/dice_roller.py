from pixels import *
from time import sleep
from adafruit_servokit import ServoKit
import json
import traceback
import asyncio


class FlipFlopServoController:
    """ Customized servo controller for the dice roller, setting the angles appropriately"""

    servo_0 = 8
    servo_90 = 98
    servo_180 = 188
    servo_index = 1

    def __init__(self):
        # Initialize the servo shield
        self.kit = ServoKit(channels=16)

        # set pwm range to something that matches our actual servo
        self.kit.servo[FlipFlopServoController.servo_index].actuation_range = 200.5
        self.kit.servo[FlipFlopServoController.servo_index].set_pulse_width_range(600, 2485)

        # start by moving the servo to 90 degrees
        self.kit.servo[FlipFlopServoController.servo_index].angle = FlipFlopServoController.servo_90
        self.flipflop = False

        # wait for servo to reach that 90 degrees
        sleep(2)

    def flip(self):
        # trigger servo
        if self.flipflop == True:
            self.kit.servo[FlipFlopServoController.servo_index].angle = FlipFlopServoController.servo_0
        else:
            self.kit.servo[FlipFlopServoController.servo_index].angle = FlipFlopServoController.servo_180
        self.flipflop = not self.flipflop


class DiceRoller:

    """ Rolls Dice and collects stats! """

    def __init__(self, dice):
        self.servo = FlipFlopServoController()
        self.dice = dice

    async def roll_once(self):

        # roll the servo
        self.servo.flip()

        # wait just a bit for the dice to start rolling
        await asyncio.sleep(0.5)

        # now wait for the dice to report a new face!
        for d in self.dice:
            await d._wait_until(lambda: d.face_up != 0, 10)

        # order according to the dice order
        ret = [0 for _ in self.dice]
        for i in range(len(self.dice)):
            ret[i] = self.dice[i].face_up
        return ret
        


async def main():
    Pixels.start()

    rollingDiceNames = ["D_48"]
    rollingDice = []

    for d in rollingDiceNames:
        print(f"Found Rolling dice: {d}")
        rollingDice.append(await Pixels.connect_by_name(d))

    if len(rollingDice) != len(rollingDiceNames):
        print("Missing some rolling dice")
        exit()

    start_time = time.perf_counter()
    print(f"Begining Rolling Test - start time: {int(start_time)}")
    
    # Roll dice a bunch
    roller = DiceRoller(rollingDice)

    # Store battery levels over time
    battery_stats = {}
    battery_stats["dice_names"] = [d.name for d in rollingDice]
    battery_stats["stats"] = []

    # store every roll
    all_rolls = {}
    all_rolls["dice_names"] = [d.name for d in rollingDice]
    all_rolls["stats"] = []
    all_rolls_stats = all_rolls["stats"]
    for d in rollingDice:
        all_rolls_stats.append([])

    # store roll stats
    roll_stats = []
    for d in rollingDice:
        die_stats = {}
        die_stats["name"] = d.name
        die_stats["stats"] = [0 for _ in range(20)]
        roll_stats.append(die_stats)

    # turn off LEDs for more battery life
    for d in rollingDice:
        d.set_led_anim_state()

    try:
        roll_count = 0
        while True:
            if roll_count % 5 == 0:
                # every 5 rolls we grab batery level and save all stats to disk

                # grab battery level for all dice
                bstats = []
                bstats.append(time.perf_counter())
                for d in rollingDice:
                    bstats.append(await d.refresh_battery_voltage())
                battery_stats["stats"].append(bstats)

                # write stats to disk
                with open("battery_stats.json", "w+") as f:
                    f.write(json.dumps(battery_stats))
                with open("all_rolls.json", "w+") as f:
                    f.write(json.dumps(all_rolls))
                with open("roll_stats.json", "w+") as f:
                    f.write(json.dumps(roll_stats))

                # print out roll stats to console
                print(f"Roll count: {roll_count}")
                print(f"Battery levels: {bstats}")
                print(roll_stats)

            # every iteration, roll dice and keep track of stats
            rolls = await roller.roll_once()
            for r in range(len(rolls)):
                roll = rolls[r]
                all_rolls_stats[r].append(roll)
                roll_stats[r]["stats"][roll - 1] += 1 # roll is face number, we convert it to an index

            # next roll
            roll_count += 1

    except:
        # dice ran out of battery...
        print(traceback.format_exc())
        pass

    end_time = time.perf_counter()
    delta_time = end_time - start_time
    print(f"Rolling Test Ended - end time: {int(end_time)} - test duration: {int(delta_time)}")

    if not Pixels.is_in_interpreter():
        Pixels.terminate()


if __name__ == "__main__":
    asyncio.run(main())

    
