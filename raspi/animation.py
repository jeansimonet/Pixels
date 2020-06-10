from enum import IntEnum, unique
import itertools

from color import Color32, remap_color
from utils import integer_to_bytes


@unique
class SpecialColor(IntEnum):
    None_ = 0
    Face = 1        # Uses the color of the face (based on a rainbow)
    ColorWheel = 2  # Uses how hot the die is (based on how much its being shaken)
    HeatCurrent = 3 # Uses the current 'heat' value to determine color
    HeatStart = 4   # Evaluate the color based on heat only once at the start of the animation


@unique
class AnimationEvent(IntEnum):
    None_ = 0
    Hello = 1
    Connected = 2
    Disconnected = 3
    LowBattery = 4
    ChargingStart = 5
    ChargingDone = 6
    ChargingError = 7
    Handling = 8
    Rolling = 9
    OnFace_Default = 10
    OnFace_00 = 11
    OnFace_01 = 12
    OnFace_02 = 13
    OnFace_03 = 14
    OnFace_04 = 15
    OnFace_05 = 16
    OnFace_06 = 17
    OnFace_07 = 18
    OnFace_08 = 19
    OnFace_09 = 20
    OnFace_10 = 21
    OnFace_11 = 22
    OnFace_12 = 23
    OnFace_13 = 24
    OnFace_14 = 25
    OnFace_15 = 26
    OnFace_16 = 27
    OnFace_17 = 28
    OnFace_18 = 29
    OnFace_19 = 30
    Crooked = 31
    Battle_ShowTeam = 32
    Battle_FaceUp = 33
    Battle_WaitingForBattle = 34
    Battle_Duel = 35
    Battle_DuelWin = 36
    Battle_DuelLose = 37
    Battle_DuelDraw = 38
    Battle_TeamWin = 39
    Battle_TeamLoose = 40
    Battle_TeamDraw = 41
    AttractMode = 42
    Heat = 43


class Animation:
    """
    ushort duration; // in ms
    ushort tracksOffset; // offset into a global buffer of tracks
    ushort trackCount;
    byte animationEvent; // Die.AnimationEvent
    byte specialColorType; // is really SpecialColor
    """
    def __init__(self):
        pass
    def pack(self):
        return bytes(
            integer_to_bytes(self.duration, 2) +
            integer_to_bytes(self.tracksOffset, 2) +
            integer_to_bytes(self.trackCount, 2) +
            integer_to_bytes(self.animationEvent, 1) +
            integer_to_bytes(self.specialColorType, 1))


class AnimationTrack:
    """
    ushort trackOffset; // offset into a global keyframe buffer
    byte ledIndex;   // 0 - 20
    byte padding;
    """
    def __init__(self):
        pass
    def pack(self):
        return bytes(
            integer_to_bytes(self.trackOffset, 2) +
            integer_to_bytes(self.ledIndex, 1) +
            [0]) # padding


class RGBTrack:
    """
    ushort keyframesOffset; // offset into a global keyframe buffer
    byte keyFrameCount;      // Keyframe count
    byte padding;
    """
    def __init__(self):
        pass
    def __hash__(self):
        return hash(tuple(self))
    def __eq__(self, other):
        return tuple(self) == tuple(other)
    def __iter__(self):
        yield self.keyframesOffset
        yield self.keyFrameCount
    def GetKeyframe(self, animSet, keyframeIndex):
        assert(keyframeIndex < self.keyFrameCount)
        return animSet.keyframes[self.keyframesOffset + keyframeIndex]
    def pack(self):
        return bytes(
            integer_to_bytes(self.keyframesOffset, 2) +
            integer_to_bytes(self.keyFrameCount, 1) +
            [0]) # padding


class RGBKeyframe:
    """
    ushort timeAndColor
    """
    def __init__(self):
        pass
    def __hash__(self):
        return hash(self.timeAndColor)
    def __eq__(self, other):
        return self.timeAndColor == other.timeAndColor
    def pack(self):
        return integer_to_bytes(self.timeAndColor, 2)


class EditKeyframe:
    """
    float time = -1;
    Color32 color;
    """
    def __init__(self, json):
        self.time = json['time']
        self.color = Color32.create(json['color'])
    def __hash__(self):
        return hash(self.time, *tuple(self.color))
    def __eq__(self, other):
        return self.time == other.time and self.color == other.color


class EditTrack:
    def __init__(self, json):
        self.ledIndex = json['ledIndex'] if 'ledIndex' in json else -1
        self.ledIndices = json['ledIndices'] if 'ledIndices' in json else []
        self.keyframes = [EditKeyframe(t) for t in json['keyframes']] if 'keyframes' in json else []

    @property
    def empty(self):
        return len(self.keyframes) == 0

    @property
    def duration(self):
        return max(k.time for k in self.keyframes) if self.keyframes else 0

    @property
    def firstTime(self):
        return self.keyframes[0].time if self.keyframes else 0

    @property
    def lastTime(self):
        return self.keyframes[-1].time if self.keyframes else 0


class EditAnimation:
    def __init__(self, json):
        self.name = json['name'] if 'name' in json else ''
        self.event = json['event'] if 'event' in json else 0
        self.specialColorType = SpecialColor(json['specialColorType']) if 'specialColorType' in json else SpecialColor.None_
        self.tracks = [EditTrack(t) for t in json['tracks']] if 'tracks' in json else []

    @property
    def empty(self):
        return len(self.tracks) == 0

    @property
    def duration(self):
        return 0 if self.empty else max(t.duration for t in self.tracks)


class AnimationSet:

    MAX_COLOR_MAP_SIZE = 1 << 7
    MAX_PALETTE_SIZE = MAX_COLOR_MAP_SIZE * 3
    SPECIAL_COLOR_INDEX = MAX_COLOR_MAP_SIZE - 1

    def __init__(self):
        pass

    @classmethod
    def from_json_obj(cls, json):
        anim = cls()
        anim.assign_animations([EditAnimation(a) for a in json['animations']])
        return anim

    @classmethod
    def from_json_file(cls, filename):
        with open(filename) as f:
            from json import load
            anim = cls.from_json_obj(load(f))
            print(f'Loaded {len(anim.animations)} animations from file: {filename}')
            return anim

    def assign_animations(self, animations):
        # Collect all colors used and stuff them in the palette
        colors = []
        for anim in animations:
            for animTrack in anim.tracks:
                for keyframe in animTrack.keyframes:
                    color = keyframe.color
                    # Note: colors with alpha set to zero are special
                    if color.a != 0 and not color in colors:
                        colors.append(color)
        # Copy r,g,b components in flat list
        self.palette = [comp for color in [list(remap_color(c))[:3] for c in colors] for comp in color]

        self.animations = []
        self.tracks = []
        self.rgb_tracks = []
        self.keyframes = []
        self.heat_track_index = -1

        # Add animations
        def ushort(i):
            assert(i < 2**16)
            return int(i)
        def byte(i):
            assert(i < 2**8)
            return int(i)
        currentTrackOffset = 0
        currentKeyframeOffset = 0
        for animIndex in range(len(animations)):
            editAnim = animations[animIndex]
            anim = Animation()
            anim.duration = ushort(editAnim.duration * 1000)
            anim.tracksOffset = ushort(currentTrackOffset)
            anim.trackCount = ushort(sum(len(t.ledIndices) for t in editAnim.tracks))
            anim.animationEvent = byte(editAnim.event)
            anim.specialColorType = byte(editAnim.specialColorType)
            self.animations.append(anim)

            if editAnim.event == AnimationEvent.Heat:
                self.heat_track_index = currentTrackOffset

            # Now add tracks
            for j in range(len(editAnim.tracks)):
                editTrack = editAnim.tracks[j]
                for led in editTrack.ledIndices:
                    track = AnimationTrack()
                    rgbTrack = RGBTrack()
                    rgbTrack.keyframesOffset = ushort(currentKeyframeOffset)
                    rgbTrack.keyFrameCount = byte(len(editTrack.keyframes))
                    track.trackOffset = ushort(len(self.rgb_tracks))
                    self.rgb_tracks.append(rgbTrack)

                    track.ledIndex = byte(led)

                    # Now add keyframes
                    for editKeyframe in editTrack.keyframes:
                        colorIndex = AnimationSet.SPECIAL_COLOR_INDEX
                        if editKeyframe.color.a != 0:
                            colorIndex = colors.index(editKeyframe.color)
                        keyframe = RGBKeyframe()
                        time = ushort(editKeyframe.time * 1000)
                        color = ushort(colorIndex)
                        keyframe.timeAndColor = ushort(((int(time / 20) & 0b111111111) << 7) | (int(colorIndex) & 0b1111111))
                        self.keyframes.append(keyframe)

                    currentKeyframeOffset += len(editTrack.keyframes)

                    self.tracks.append(track)

            currentTrackOffset += sum(len(t.ledIndices) for t in editAnim.tracks)

        self.compress()

    def compress(self):
        def ushort(i):
            assert(i < 2**16)
            return int(i)
        # First try to find identical sets of keyframes in tracks
        for t in range(len(self.rgb_tracks)):
            trackT = self.rgb_tracks[t]
            for r in range(t + 1, len(self.rgb_tracks)):
                trackR = self.rgb_tracks[r]
                # Only try to collapse tracks that are not exactly the same
                if trackT != trackR:
                    if trackR.keyFrameCount == trackT.keyFrameCount:
                        # Compare actual keyframes
                        kfEquals = True
                        for k in range(trackR.keyFrameCount):
                            kfRk = trackR.GetKeyframe(self, k)
                            kfTk = trackT.GetKeyframe(self, k)
                            if kfRk != kfTk:
                                kfEquals = False
                                break

                        if kfEquals:
                            # Sweet, we can compress the keyframes
                            # Fix up any other tracks
                            for i in range(len(self.rgb_tracks)):
                                tr = self.rgb_tracks[i]
                                if tr.keyframesOffset > trackR.keyframesOffset:
                                    tr.keyframesOffset -= trackR.keyFrameCount
                                    self.rgb_tracks[i] = tr

                            # Remove the duplicate keyframes
                            newKeyframes = []
                            for i in range(trackR.keyframesOffset):
                                newKeyframes.append(self.keyframes[i])
                            for i in range(trackR.keyframesOffset + trackR.keyFrameCount, len(self.keyframes)):
                                newKeyframes.append(self.keyframes[i])
                            self.keyframes = newKeyframes

                            # And make R point to the keyframes of T
                            trackR.keyframesOffset = trackT.keyframesOffset
                            self.rgb_tracks[r] = trackR

        # Then remove duplicate RGB tracks
        t = 0
        while t < len(self.rgb_tracks):
            trackT = self.rgb_tracks[t]
            r = t + 1
            while r < len(self.rgb_tracks):
                trackR = self.rgb_tracks[r]
                if trackR == trackT:
                    # Remove track R and fix anim tracks
                    # Fix up other animation tracks
                    for j in range(len(self.tracks)):
                        trj = self.tracks[j]
                        if trj.trackOffset == r:
                            trj.trackOffset = ushort(t)
                        elif trj.trackOffset > r:
                            trj.trackOffset -= 1

                    if r == self.heat_track_index:
                        self.heat_track_index = ushort(t)
                    elif r < self.heat_track_index:
                        self.heat_track_index -= 1

                    # Remove the duplicate RGBTrack
                    newRGBTracks = []
                    for j in range(r):
                        newRGBTracks.append(self.rgb_tracks[j])
                    for j in range(r + 1, len(self.rgb_tracks)):
                        newRGBTracks.append(self.rgb_tracks[j])
                    self.rgb_tracks = newRGBTracks
                r += 1
            t += 1
        # We should also remove duplicate anim tracks and animation
        
    def pack(self):
        data = []
        data += self.palette
        for kf in self.keyframes:
            data += kf.pack()
        for t in self.rgb_tracks:
            data += t.pack()
        for t in self.tracks:
            data += t.pack()
        for a in self.animations:
            data += a.pack()
        return data

if __name__ == "__main__":
    AnimationSet.from_json_file('animation_set.json').pack()
    AnimationSet.from_json_file('D20_animation_set.json').pack()
    # with open("anim.bin", "wb") as f:
    #     f.write(bytearray(b))
