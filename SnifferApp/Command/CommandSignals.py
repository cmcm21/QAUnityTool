from enum import Enum


class CommandSignal(Enum):
    RECORD = "RECORD"
    STOP_REC = "STOP_REC"
    REPLAY = "REPLAY"
    STOP_REPLAY = "STOP_REPLAY"
    SEND_FILE = "SEND_FILE"
    GET_FILE = "GET_FILE"
    GET_DEVICE_DATA = "GET_DEVICE_DATA"
    CHANGE_STATE = "CHANGE_STATE"
