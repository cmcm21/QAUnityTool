import asyncio
import time

from PySide6 import QtWidgets, QtGui


class SplashScreen(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        pixImage = QtGui.QPixmap("../Assets/loadingImage.png")
        self.splashScreen = QtWidgets.QSplashScreen(pixmap=pixImage)

    def setSplashScreen(self, msg: str):
        self.show()
        self.splashScreen.showMessage(msg)

    async def customHide(self, window: QtWidgets.QMainWindow):
        self.splashScreen.finish(window)

    async def _hide(self):
        await asyncio.sleep(2)
        self.hide()
