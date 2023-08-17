from PySide6 import QtCore, QtGui, QtWidgets
from Network.DeviceManager import DeviceManager
from Utils.Events import Event


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
        self.deviceSelectedChanged = Event()

        (self.devicesListWidget.
         itemSelectionChanged.connect(lambda: self.deviceSelectedChanged(device=self._getFirstSelectedDevice())))

    def _getFirstSelectedDevice(self) -> DeviceManager | None:
        selectedItems = self.devicesListWidget.selectedItems()
        if len(selectedItems) == 0:
            return None
        return selectedItems[0].data(QtCore.Qt.ItemDataRole.UserRole)

    def getAllSelectedDevices(self) -> list[DeviceManager]:
        devices = []
        for item in self.devicesListWidget.selectedItems():
            devices.append(item.data(QtCore.Qt.ItemDataRole.UserRole))

        return devices

    def _init_layout(self):
        self.layout = QtWidgets.QVBoxLayout(self)
        self.layout.addWidget(self.titleLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.layout.addWidget(self.devicesListWidget)
        self.layout.addWidget(self.initServerButton)

    def addDevice(self, device: DeviceManager):
        item = QtWidgets.QListWidgetItem("Device Connected: " + str(device.address), self.devicesListWidget)
        item.setData(QtCore.Qt.ItemDataRole.UserRole, device)

    def removeDevice(self, device: DeviceManager):
        items = self.devicesListWidget.findItems(
            "Device Connected: " + str(device.address), QtCore.Qt.MatchFlag.MatchWildcard)

        if len(items) > 0:
            self.devicesListWidget.removeItemWidget(items[0])

        return
