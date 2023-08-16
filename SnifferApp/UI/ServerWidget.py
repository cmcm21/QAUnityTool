from PySide6 import QtCore, QtGui, QtWidgets
from Network.DeviceManager import DeviceManager


class ServerWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self._init_widgets()
        self._init_layout()
        self.initServerAction = None

    def _init_widgets(self):
        self.titleLabel = QtWidgets.QLabel("Devices")
        self.initServerButton = QtWidgets.QPushButton("Init Server")
        self.devicesListWidget = QtWidgets.QListWidget()

    def _init_layout(self):
        self.layout = QtWidgets.QVBoxLayout(self)
        self.layout.addWidget(self.titleLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.layout.addWidget(self.devicesListWidget)
        self.layout.addWidget(self.initServerButton)

    def addDevice(self, device: DeviceManager):
        item = QtWidgets.QListWidgetItem("Device Connected: " + str(device.address))
        self.devicesListWidget.addItem(item)
