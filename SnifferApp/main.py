import sys
from Hub.SnifferHub import SnifferHub

if __name__ == '__main__':
    snifferHub = SnifferHub()
    sys.exit(snifferHub.app())
