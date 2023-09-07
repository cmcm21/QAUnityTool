from PySide6 import QtCore, QtWidgets
from Network.Clients.DeviceClient import DeviceClient
from Utils.Events import Event
from Hub.SnifferState import SnifferState


class ServerWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self._initWidgets()
        self._initLayout()
        self.initServerAction = None

    def _initWidgets(self):
        self.titleLabel = QtWidgets.QLabel("Devices")
        self.initServerButton = QtWidgets.QPushButton("Init Server")
        self.devicesListWidget = QtWidgets.QListWidget()
        self.devicesListWidget.setSelectionMode(QtWidgets.QListWidget.SelectionMode.MultiSelection)
        self.selectedDevices = None
        self.devicesSelectedChanged = Event()
        self.clearSelectionEvent = Event()
        self.devicesListWidget.itemSelectionChanged.connect(self._onSelectionChanged)

    def _onSelectionChanged(self):
        selectedDevices = self._getSelectedDevices()
        if len(selectedDevices) > 0 and self.selectedDevices != selectedDevices:
            self.devicesSelectedChanged(devices=selectedDevices)
            self.selectedDevices = selectedDevices

    def _getSelectedDevices(self) -> list[DeviceClient]:
        selectedDevices = []
        selectedItems = self.devicesListWidget.selectedItems()
        for item in selectedItems:
            selectedDevices.append(item.data(QtCore.Qt.ItemDataRole.UserRole))

        return selectedDevices

    def _initLayout(self):
        self.layout = QtWidgets.QVBoxLayout()
        self.layout.addWidget(self.titleLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.layout.addWidget(self.devicesListWidget)
        self.layout.addWidget(self.initServerButton)
        self.setMaximumSize(QtCore.QSize(300, 400))
        self.setMinimumSize(QtCore.QSize(300, 400))
        self.setLayout(self.layout)

    def addDevice(self, device: DeviceClient):
        itemText = f"[ {str(device.hostname)}, {device.address} ]"
        item = QtWidgets.QListWidgetItem(itemText, self.devicesListWidget)
        item.setData(QtCore.Qt.ItemDataRole.UserRole, device)
        if self.devicesListWidget.count() == 1:
            item.setSelected(True)
            self._onSelectionChanged()

    def updateDevice(self, *args, **kwargs):
        if 'device' in kwargs:
            device = kwargs['device']
            self.removeDevice(device)
            self.addDevice(device)

    def removeDevice(self, device: DeviceClient):
        items = self.findItems(device)

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

    def findItems(self, device: DeviceClient):
        items = self.devicesListWidget.findItems(f"{device.address}", QtCore.Qt.MatchFlag.MatchContains)

        return items

    def hubStateChanged(self, appState: SnifferState):
        self.devicesListWidget.setEnabled(appState == SnifferState.IDLE)
