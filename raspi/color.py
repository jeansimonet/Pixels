import sys


class Color32:
    """8 bit ARGB color, with component stored as integers in [0, 255]"""

    def __init__(self, r: int, g: int, b: int, a: int = 0):
        def check(x, name):
            if not isinstance(x, int): raise Exception(f"Argument '{name}' must of type int")
            if x < 0 or x > 255: raise Exception(f"Argument '{name}' with value {x} is out of bounds")
            return x
        self.r = check(r, 'r')
        self.g = check(g, 'g')
        self.b = check(b, 'b')
        self.a = check(a, 'a')

    @classmethod
    def create(cls, obj):
        if isinstance(obj, dict):
            a = obj['a'] if 'a' in obj else 0
            return cls(obj['r'], obj['g'], obj['b'], a)
        elif isinstance(obj, (list, tuple)):
            a = obj[3] if len(obj) > 3 else 0
            return cls(obj[0], obj[1], obj[2], a)
        else:
            raise Exception('Argument must be a dict, a list or a tuple')

    def to_rgb(self):
        return ((self.r & 0xff) << 16) + ((self.g & 0xff) << 8) + (self.b & 0xff)

    def __hash__(self):
        return hash(tuple(self))

    def __eq__(self, other):
        return tuple(self) == tuple(other)

    def __repr__(self):
        return 'Color32' + repr(tuple(self))

    def __iter__(self):
        yield self.r
        yield self.g
        yield self.b
        yield self.a


Color32.black = Color32(0, 0, 0)
Color32.white = Color32(255, 255, 255)


def _load_color_mapping_data(filename):
    with open(filename) as f:
        from json import load
        json = load(f)
        color_mapping = json['color_mapping']
        return \
            [Color32.create(c) for c in color_mapping['source_colors']], \
            [Color32.create(c) for c in color_mapping['dest_colors']], \
            json['gamma_table']


_source_colors, _dest_colors, _gamma_table = _load_color_mapping_data('color_mapping.json')


def remap_color(color: Color32):
    # Find the closest source color and return the matching mapped color
    min_dist = sys.maxsize
    for i in range(len(_source_colors)):
        src = _source_colors[i]
        dist  = (color.r - src.r)**2 + (color.g - src.g)**2 + (color.b - src.b)**2
        if dist < min_dist:
            min_dist = dist
            closest = _dest_colors[i]

    # Inverse gamma corrects the color, so that it looks nice on the die
    mapped = list(closest)
    for i in range(3):
        if mapped[i] != 0 and mapped[i] != 255:
            index = 255
            while _gamma_table[index] > mapped[i]:
                index -= 1
            mapped[i] = index

    return Color32.create(mapped)


# def inverse_remap(color: Color32):
#     min_dist = sys.maxsize
#     for i in range(len(_dest_colors)):
#         dst = _dest_colors[i]
#         dist  = (color.r - dst.r)**2 + (color.g - dst.g)**2 + (color.b - dst.b)**2
#         if dist < min_dist:
#             min_dist = dist
#             closest = _source_colors[i]
#     return closest


if __name__ == "__main__":
    assert(remap_color(Color32(60, 3, 120)) == Color32(43, 0, 96, 255))
    # assert(inverse_remap(Color32(2, 0, 20)) == Color32(58, 0, 128, 255))
