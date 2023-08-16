class Event(object):

    def __init__(self):
        self.__eventHandlers = []

    def __iadd__(self, handler):
        self.__eventHandlers.append(handler)
        return self

    def __isub__(self, handler):
        self.__eventHandlers.remove(handler)
        return self

    def __call__(self, *args, **keywords):
        for eventhandler in self.__eventHandlers:
            eventhandler(*args, **keywords)

    def __del__(self):
        self.__eventHandlers.clear()