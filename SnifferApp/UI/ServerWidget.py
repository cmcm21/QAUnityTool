from PySide6 import QtCore, QtWidgets
from Network.Clients.DeviceClient import DeviceClient
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
        self.selectedDevice = None
        self.deviceSelectedChanged = Event()
        self.clearSelectionEvent = Event()
        self.devicesListWidget.itemSelectionChanged.connect(self._onSelectionChanged)

    def _onSelectionChanged(self):
        deviceSelected = self._getFirstSelectedDevice()

        if deviceSelected is not None and self.selectedDevice != deviceSelected:
            self.deviceSelectedChanged(device=deviceSelected)
            self.selectedDevice = deviceSelected
        return

    def _getFirstSelectedDevice(self) -> DeviceClient | None:
        selectedItems = self.devicesListWidget.selectedItems()
        if len(selectedItems) == 0:
            return None
        return selectedItems[0].data(QtCore.Qt.ItemDataRole.UserRole)

    def getAllSelectedDevices(self) -> list[DeviceClient]:
        devices = []
        for item in self.devicesListWidget.selectedItems():
            devices.append(item.data(QtCore.Qt.ItemDataRole.UserRole))

        return devices

    def _init_layout(self):
        self.layout = QtWidgets.QVBoxLayout(self)
        self.layout.addWidget(self.titleLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.layout.addWidget(self.devicesListWidget)
        self.layout.addWidget(self.initServerButton)

    def addDevice(self, device: DeviceClient):
        item = QtWidgets.QListWidgetItem("Device Connected: " + str(device.address), self.devicesListWidget)
        item.setData(QtCore.Qt.ItemDataRole.UserRole, device)
        if self.devicesListWidget.count() == 1:
            item.setSelected(True)
            self._onSelectionChanged()

    def removeDevice(self, device: DeviceClient):
        items = self.devicesListWidget.findItems(
            "Device Connected: " + str(device.address), QtCore.Qt.MatchFlag.MatchWildcard)

        if len(items) <= 0:
            return

        if len(items) > 0:
            itemRow = self.devicesListWidget.row(items[0])
            self.devicesListWidget.removeItemWidget(items[0])
            self.devicesListWidget.takeItem(itemRow)

        if self.devicesListWidget.count() > 0:
            self.devicesListWidget.itemAt(QtCore.QPoint(0, 0)).setSelected(True)
            self._onSelectionChanged()

        if items[0].isSelected():
            self.devicesListWidget.clearSelection()
            self.clearSelectionEvent()
