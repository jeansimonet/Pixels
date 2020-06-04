
def integer_to_bytes(integer, size):
    return [(integer >> i & 0xff) for i in range(0, 8 * size, 8)]

# Simple event class helper
class Event:
    def __init__(self):
        self._callbacks = []

    def notify(self, *args, **kwargs):
        for cb in self._callbacks:
            cb(*args, **kwargs)

    def attach(self, callback):
        self._callbacks.append(callback)

    def detach(self, callback):
        self._callbacks.remove(callback)
