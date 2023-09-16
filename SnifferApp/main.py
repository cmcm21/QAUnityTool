import sys
import faulthandler
from Hub.SnifferHub import SnifferHub

if __name__ == '__main__':
    faulthandler.enable()
    snifferHub = SnifferHub()
    snifferHub.app()
