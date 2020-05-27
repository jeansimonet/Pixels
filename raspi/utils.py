
def integer_to_bytes(integer, size):
    return [(integer >> i & 0xff) for i in range(0, 8 * size, 8)]
