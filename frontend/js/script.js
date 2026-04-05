const savedUser = localStorage.getItem('userEmail');
if (!savedUser) {
  window.location.href = './login.html';
}

const STORAGE_KEYS = {
    theme: 'theme',
    points: 'logistics_points',
    warehouses: 'logistics_warehouses',
    routes: 'logistics_routes',
    pointCounter: 'logistics_point_counter',
    warehouseCounter: 'logistics_warehouse_counter',
    routeCounter: 'logistics_route_counter',
    selectedPointId: 'logistics_selected_point_id',
    selectedWarehouseId: 'logistics_selected_warehouse_id',
    lastRoutePrice: 'logistics_last_route_price',
    jsonSeeded: 'logistics_json_seeded'
  };

const ROUTE_COLORS = [
  '#22c55e',
  '#3b82f6',
  '#f59e0b',
  '#ef4444',
  '#a855f7',
  '#06b6d4',
  '#ec4899',
  '#84cc16',
  '#f97316',
  '#14b8a6'
];

const FUEL_CONFIG = {
  diesel: {
    label: 'Дизель',
    consumption: 8.5,
    unit: 'л / 100 км'
  },
  petrol: {
    label: 'Бензин',
    consumption: 9.5,
    unit: 'л / 100 км'
  },
  gas: {
    label: 'Газ',
    consumption: 11.5,
    unit: 'л / 100 км'
  },
  electric: {
    label: 'Електро',
    consumption: 19,
    unit: 'кВт·год / 100 км'
  }
};

const LOAD_FACTORS = {
  light: { label: 'Легка', multiplier: 0.95 },
  medium: { label: 'Середня', multiplier: 1 },
  heavy: { label: 'Висока', multiplier: 1.15 }
};

const OPERATOR_NAMES = [
  'Amic',
  'Bvs',
  'BrentOil',
  'Chipo',
  'Euro5',
  'GrandPetrol',
  'GreenWave',
  'Klo',
  'Mango',
  'Marshal',
  'Motto',
  'Neftek',
  'Ovis',
  'Parallel',
  'Rls',
  'Rodnik',
  'Socar',
  'SunOil',
  'UGo',
  'Ukrnafta',
  'Upg',
  'Vst',
  'VostokGaz',
  'Wog',
  'Zog',
  'Avantazh7',
  'Avtotrans',
  'BrsmNafta',
  'Dnipronafta',
  'Katral',
  'Kvorum',
  'Market',
  'Okko',
  'Olas',
  'RurGrup',
  'Svoi',
  'Faktor'
].sort((a, b) => a.localeCompare(b, 'uk', { sensitivity: 'base' }));

const body = document.body;
const lightBtn = document.getElementById('lightBtn');
const darkBtn = document.getElementById('darkBtn');
const logoutBtn = document.getElementById('logoutBtn');
const addPointBtn = document.getElementById('addPointBtn');
const addWarehouseBtn = document.getElementById('addWarehouseBtn');
const buildRouteBtn = document.getElementById('buildRouteBtn');
const waypointRouteBtn = document.getElementById('waypointRouteBtn');
const finishWaypointRouteBtn = document.getElementById('finishWaypointRouteBtn');
const clearWaypointRouteBtn = document.getElementById('clearWaypointRouteBtn');
const waypointControls = document.getElementById('waypointControls');
const waypointCounterText = document.getElementById('waypointCounterText');
const priceBtn = document.getElementById('priceBtn');
const modeInfo = document.getElementById('modeInfo');
const pointsList = document.getElementById('pointsList');
const warehousesList = document.getElementById('warehousesList');
const routesList = document.getElementById('routesList');
const selectedPointText = document.getElementById('selectedPointText');
const selectedWarehouseText = document.getElementById('selectedWarehouseText');
const routePriceText = document.getElementById('routePriceText');
const routePriceMeta = document.getElementById('routePriceMeta');

const priceModalBackdrop = document.getElementById('priceModalBackdrop');
const closePriceModalBtn = document.getElementById('closePriceModalBtn');
const cancelPriceModalBtn = document.getElementById('cancelPriceModalBtn');
const priceForm = document.getElementById('priceForm');
const routeNameInput = document.getElementById('routeNameInput');
const warehouseAddressInput = document.getElementById('warehouseAddressInput');
const pointAddressInput = document.getElementById('pointAddressInput');
const fuelTypeSelect = document.getElementById('fuelTypeSelect');
const fuelOperatorSelect = document.getElementById('fuelOperatorSelect');
const vehicleLoadSelect = document.getElementById('vehicleLoadSelect');
const fuelInfoCard = document.getElementById('fuelInfoCard');
const submitPriceBtn = document.getElementById('submitPriceBtn');
const priceModalTitle = document.getElementById('priceModalTitle');
const priceModalSubtitle = document.getElementById('priceModalSubtitle');
const waypointNote = document.getElementById('waypointNote');

let currentMode = 'view';
let pointCounter = Number(localStorage.getItem(STORAGE_KEYS.pointCounter)) || 1;
let warehouseCounter = Number(localStorage.getItem(STORAGE_KEYS.warehouseCounter)) || 1;
let routeCounter = Number(localStorage.getItem(STORAGE_KEYS.routeCounter)) || 1;

let points = [];
let warehouses = [];
let routes = [];

let selectedPointId = parseStoredNumber(localStorage.getItem(STORAGE_KEYS.selectedPointId));
let selectedWarehouseId = parseStoredNumber(localStorage.getItem(STORAGE_KEYS.selectedWarehouseId));
let activeRouteId = null;
let editingRouteId = null;
let modalMode = 'create-from-address';

let tempWaypointMarkers = [];
let tempWaypointCoords = [];

let carMarker = null;
let carAnimationInterval = null;

const OPERATOR_CONFIG = buildOperatorsConfig();

const map = L.map('map').setView([49.84, 24.03], 12);


L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
  attribution: '&copy; OpenStreetMap'
}).addTo(map);

setTimeout(() => {
  map.invalidateSize();
}, 200);

window.addEventListener('load', () => {
  setTimeout(() => {
    map.invalidateSize();
  }, 300);
});

const pointIcon = L.divIcon({
  className: '',
  html: `<div style="width:16px;height:16px;border-radius:50%;background:#3b82f6;border:3px solid white;box-shadow:0 0 10px rgba(59,130,246,0.6);"></div>`,
  iconSize: [16, 16],
  iconAnchor: [8, 8]
});

const warehouseIcon = L.divIcon({
  className: '',
  html: `<div style="width:16px;height:16px;border-radius:50%;background:#a855f7;border:3px solid white;box-shadow:0 0 10px rgba(168,85,247,0.6);"></div>`,
  iconSize: [16, 16],
  iconAnchor: [8, 8]
});

const waypointIcon = L.divIcon({
  className: '',
  html: `<div style="width:14px;height:14px;border-radius:50%;background:#f59e0b;border:3px solid white;box-shadow:0 0 10px rgba(245,158,11,0.6);"></div>`,
  iconSize: [14, 14],
  iconAnchor: [7, 7]
});

async function initApp() {
  populateOperatorSelect();
  applySavedTheme();
  attachThemeListeners();
  attachMainListeners();

  await ensureJsonSeeded();

  loadSavedData();
  restoreLastRoutePrice();
  updateSelectionText();
  renderPointsList();
  renderWarehousesList();
  renderRoutesList();
  updateFuelInfoCard();
  updateWaypointControls();

  setTimeout(() => {
    map.invalidateSize();
  }, 300);
}

function parseStoredNumber(value) {
  if (value === null || value === '') return null;
  const number = Number(value);
  return Number.isFinite(number) ? number : null;
}

function buildOperatorsConfig() {
  const base = {
    diesel: 55,
    petrol: 58,
    gas: 31,
    electric: 9
  };

  const operators = {};

  OPERATOR_NAMES.forEach((name, index) => {
    const shift = ((index % 7) - 3) * 0.35;
    const bonus = (index % 3) * 0.15;

    operators[name] = {
      label: name,
      prices: {
        diesel: roundPrice(base.diesel + shift + bonus),
        petrol: roundPrice(base.petrol + shift + bonus + 0.4),
        gas: roundPrice(base.gas + shift * 0.4),
        electric: roundPrice(base.electric + (index % 5) * 0.2)
      }
    };
  });

  return operators;
}

function roundPrice(value) {
  return Number(value.toFixed(2));
}

function populateOperatorSelect() {
  fuelOperatorSelect.innerHTML = '';

  OPERATOR_NAMES.forEach(name => {
    const option = document.createElement('option');
    option.value = name;
    option.textContent = name;
    fuelOperatorSelect.appendChild(option);
  });

  fuelOperatorSelect.value = OPERATOR_NAMES.includes('Okko') ? 'Okko' : OPERATOR_NAMES[0];
}

function attachThemeListeners() {
  if (lightBtn) {
    lightBtn.addEventListener('click', () => {
      body.classList.remove('dark');
      localStorage.setItem(STORAGE_KEYS.theme, 'light');
    });
  }

  if (darkBtn) {
    darkBtn.addEventListener('click', () => {
      body.classList.add('dark');
      localStorage.setItem(STORAGE_KEYS.theme, 'dark');
    });
  }

  if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
      localStorage.removeItem('userEmail');
      localStorage.removeItem('userName');
      window.location.href = './login.html';
    });
  }
}

function applySavedTheme() {
  const savedTheme = localStorage.getItem(STORAGE_KEYS.theme);
  if (savedTheme === 'light') {
    body.classList.remove('dark');
  } else {
    body.classList.add('dark');
  }
}

function attachMainListeners() {
  addPointBtn.addEventListener('click', () => {
    cancelWaypointMode();
    currentMode = 'add-point';
    modeInfo.textContent = 'Режим: додавання точки. Клікни по карті.';
  });

  addWarehouseBtn.addEventListener('click', () => {
    cancelWaypointMode();
    currentMode = 'add-warehouse';
    modeInfo.textContent = 'Режим: додавання складу. Клікни по карті.';
  });

  buildRouteBtn.addEventListener('click', () => {
    const warehouse = warehouses.find(w => w.id === selectedWarehouseId);
    const point = points.find(p => p.id === selectedPointId);

    if (!warehouse || !point) {
      modeInfo.textContent = 'Спочатку обери 1 склад і 1 точку.';
      return;
    }

    cancelWaypointMode();
    openPriceModalForSelectedPair(warehouse, point, false);
  });

  waypointRouteBtn.addEventListener('click', () => {
    const warehouse = warehouses.find(w => w.id === selectedWarehouseId);
    const point = points.find(p => p.id === selectedPointId);

    if (!warehouse || !point) {
      modeInfo.textContent = 'Спочатку обери склад і точку, а тоді додавай проміжні точки.';
      return;
    }

    currentMode = 'add-waypoints';
    clearTemporaryWaypoints();
    updateWaypointControls();
    modeInfo.textContent = 'Режим проміжних точок: клікай по карті, щоб додати точки маршруту. Щоб видалити одну точку — клікни по ній.';
  });

  finishWaypointRouteBtn.addEventListener('click', () => {
    const warehouse = warehouses.find(w => w.id === selectedWarehouseId);
    const point = points.find(p => p.id === selectedPointId);

    if (!warehouse || !point) {
      modeInfo.textContent = 'Не вдалося знайти склад або точку.';
      return;
    }

    if (!tempWaypointCoords.length) {
      modeInfo.textContent = 'Спочатку додай хоча б одну проміжну точку.';
      return;
    }

    openPriceModalForSelectedPair(warehouse, point, true);
  });

  clearWaypointRouteBtn.addEventListener('click', () => {
    clearTemporaryWaypoints();
    modeInfo.textContent = 'Проміжні точки очищено.';
  });

  priceBtn.addEventListener('click', () => {
    cancelWaypointMode();
    openPriceModalForNewRoute();
  });

  closePriceModalBtn.addEventListener('click', closePriceModal);
  cancelPriceModalBtn.addEventListener('click', closePriceModal);
  fuelTypeSelect.addEventListener('change', updateFuelInfoCard);
  fuelOperatorSelect.addEventListener('change', updateFuelInfoCard);
  vehicleLoadSelect.addEventListener('change', updateFuelInfoCard);

  priceModalBackdrop.addEventListener('click', (e) => {
    if (e.target === priceModalBackdrop) {
      closePriceModal();
    }
  });

  priceForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    await handlePriceFormSubmit();
  });

  map.on('click', async (e) => {
    const { lat, lng } = e.latlng;

    if (currentMode === 'add-point') {
      currentMode = 'view';
      await createPoint(lat, lng);
      return;
    }

    if (currentMode === 'add-warehouse') {
      currentMode = 'view';
      await createWarehouse(lat, lng);
      return;
    }

    if (currentMode === 'add-waypoints') {
      addTemporaryWaypoint(lat, lng);
    }
  });
}

function updateFuelInfoCard() {
  const fuelType = fuelTypeSelect.value;
  const fuel = FUEL_CONFIG[fuelType];
  const load = LOAD_FACTORS[vehicleLoadSelect.value];
  const operatorName = fuelOperatorSelect.value;
  const pricePerUnit = getOperatorFuelPrice(operatorName, fuelType);
  const adjustedConsumption = fuel.consumption * load.multiplier;
  const unitShort = fuel.unit.includes('кВт') ? 'кВт·год' : 'л';

  fuelInfoCard.innerHTML = `
    <strong>${fuel.label}</strong><br>
    Оператор: <strong>${operatorName}</strong><br>
    Ціна за ${unitShort}: <strong>${formatNumber(pricePerUnit)} грн</strong><br>
    Базове споживання: <strong>${fuel.consumption} ${fuel.unit}</strong><br>
    З урахуванням завантаженості (${load.label.toLowerCase()}): <strong>${adjustedConsumption.toFixed(1)} ${fuel.unit}</strong>
  `;
}

function openPriceModalForNewRoute() {
  modalMode = 'create-from-address';
  editingRouteId = null;

  priceModalTitle.textContent = 'Розрахувати ціну за маршрутом';
  priceModalSubtitle.textContent = 'Введіть назву маршруту, адреси та параметри';
  submitPriceBtn.textContent = 'Розрахувати';

  routeNameInput.value = '';
  warehouseAddressInput.value = '';
  pointAddressInput.value = '';
  fuelTypeSelect.value = 'diesel';
  fuelOperatorSelect.value = OPERATOR_NAMES.includes('Okko') ? 'Okko' : OPERATOR_NAMES[0];
  vehicleLoadSelect.value = 'medium';
  waypointNote.classList.add('hidden');

  updateFuelInfoCard();
  priceModalBackdrop.classList.remove('hidden');
  modeInfo.textContent = 'Введіть адреси та параметри для автоматичного розрахунку маршруту.';
}

function openPriceModalForSelectedPair(warehouse, point, withWaypoints = false) {
  modalMode = withWaypoints ? 'create-from-selected-with-waypoints' : 'create-from-selected';
  editingRouteId = null;

  priceModalTitle.textContent = withWaypoints
    ? 'Маршрут через проміжні точки'
    : 'Розрахувати маршрут';

  priceModalSubtitle.textContent = withWaypoints
    ? 'Можна задати назву, пальне, оператора і побудувати маршрут через вибрані точки'
    : 'Можна змінити назву маршруту, пальне та параметри';

  submitPriceBtn.textContent = 'Побудувати маршрут';

  routeNameInput.value = `Маршрут ${warehouse.name} → ${point.name}`;
  warehouseAddressInput.value = warehouse.address;
  pointAddressInput.value = point.address;
  fuelTypeSelect.value = 'diesel';
  fuelOperatorSelect.value = OPERATOR_NAMES.includes('Okko') ? 'Okko' : OPERATOR_NAMES[0];
  vehicleLoadSelect.value = 'medium';

  if (withWaypoints) {
    waypointNote.classList.remove('hidden');
  } else {
    waypointNote.classList.add('hidden');
  }

  updateFuelInfoCard();
  priceModalBackdrop.classList.remove('hidden');
  modeInfo.textContent = withWaypoints
    ? 'Задай параметри для маршруту через проміжні точки.'
    : 'Оберіть назву, пальне, оператора та завантаженість.';
}

function openPriceModalForEdit(route) {
  modalMode = 'edit-route';
  editingRouteId = route.id;

  priceModalTitle.textContent = 'Редагувати маршрут';
  priceModalSubtitle.textContent = 'Можна змінити назву, адреси, пальне, оператора та завантаженість';
  submitPriceBtn.textContent = 'Зберегти зміни';

  const warehouse = warehouses.find(w => w.id === route.warehouseId);
  const point = points.find(p => p.id === route.pointId);

  routeNameInput.value = route.name || '';
  warehouseAddressInput.value = warehouse ? warehouse.address : '';
  pointAddressInput.value = point ? point.address : '';
  fuelTypeSelect.value = route.fuelType || 'diesel';
  fuelOperatorSelect.value = route.operatorName || (OPERATOR_NAMES.includes('Okko') ? 'Okko' : OPERATOR_NAMES[0]);
  vehicleLoadSelect.value = route.loadType || 'medium';
  waypointNote.classList.toggle('hidden', !route.waypointCoords || !route.waypointCoords.length);

  updateFuelInfoCard();
  priceModalBackdrop.classList.remove('hidden');
  modeInfo.textContent = 'Редагування маршруту.';
}

function closePriceModal() {
  priceModalBackdrop.classList.add('hidden');
  editingRouteId = null;
  modalMode = 'create-from-address';
  submitPriceBtn.textContent = 'Розрахувати';
}

async function handlePriceFormSubmit() {
  const routeName = routeNameInput.value.trim();
  const warehouseAddress = warehouseAddressInput.value.trim();
  const pointAddress = pointAddressInput.value.trim();
  const fuelType = fuelTypeSelect.value;
  const operatorName = fuelOperatorSelect.value;
  const loadType = vehicleLoadSelect.value;

  if (!routeName || !warehouseAddress || !pointAddress) {
    alert('Будь ласка, заповніть назву маршруту, адресу складу і адресу точки.');
    return;
  }

  submitPriceBtn.disabled = true;
  submitPriceBtn.textContent = 'Розраховую...';
  modeInfo.textContent = 'Шукаю адреси та будую маршрут...';

  try {
    const warehouseCoords = await geocodeAddress(warehouseAddress);
    const pointCoords = await geocodeAddress(pointAddress);

    if (!warehouseCoords || !pointCoords) {
      throw new Error('Не вдалося знайти одну з адрес. Спробуйте написати точніше.');
    }

    const warehouse = await ensureWarehouseByAddress(warehouseAddress, warehouseCoords.lat, warehouseCoords.lng);
    const point = await ensurePointByAddress(pointAddress, pointCoords.lat, pointCoords.lng);

    selectedWarehouseId = warehouse.id;
    selectedPointId = point.id;
    persistSelections();
    updateSelectionText();

    let waypointCoords = [];
    if (modalMode === 'create-from-selected-with-waypoints') {
      waypointCoords = [...tempWaypointCoords];
    }

    if (modalMode === 'edit-route' && editingRouteId) {
      const existingRoute = routes.find(route => route.id === editingRouteId);
      waypointCoords = existingRoute?.waypointCoords || [];
    }

    let route;

    if (modalMode === 'edit-route' && editingRouteId) {
      route = await updateExistingRoute(editingRouteId, {
        warehouse,
        point,
        routeName,
        fuelType,
        operatorName,
        loadType,
        waypointCoords
      });
    } else {
      route = await createOrUpdateRoute({
        warehouse,
        point,
        routeName,
        fuelType,
        operatorName,
        loadType,
        waypointCoords,
        preferExistingByPair: false
      });
    }

    setActiveRoute(route.id);
    closePriceModal();
    resetModalForm();

    if (modalMode === 'create-from-selected-with-waypoints') {
      cancelWaypointMode();
    }

    modeInfo.textContent = `Маршрут побудовано: ${route.name}`;
  } catch (error) {
    alert(error.message || 'Не вдалося розрахувати маршрут.');
    modeInfo.textContent = 'Не вдалося розрахувати маршрут. Перевір введені дані.';
  } finally {
    submitPriceBtn.disabled = false;
    submitPriceBtn.textContent =
      modalMode === 'edit-route'
        ? 'Зберегти зміни'
        : modalMode === 'create-from-address'
          ? 'Розрахувати'
          : 'Побудувати маршрут';
  }
}

function resetModalForm() {
  routeNameInput.value = '';
  warehouseAddressInput.value = '';
  pointAddressInput.value = '';
  fuelTypeSelect.value = 'diesel';
  fuelOperatorSelect.value = OPERATOR_NAMES.includes('Okko') ? 'Okko' : OPERATOR_NAMES[0];
  vehicleLoadSelect.value = 'medium';
  updateFuelInfoCard();
}

async function geocodeAddress(query) {
  const response = await fetch(
    `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&q=${encodeURIComponent(query)}`,
    { headers: { Accept: 'application/json' } }
  );

  if (!response.ok) {
    throw new Error('Помилка при пошуку адреси.');
  }

  const data = await response.json();
  if (!data.length) return null;

  return {
    lat: Number(data[0].lat),
    lng: Number(data[0].lon),
    displayName: data[0].display_name
  };
}

async function getAddressFromCoords(lat, lng) {
  try {
    const response = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}`,
      { headers: { Accept: 'application/json' } }
    );

    if (!response.ok) {
      throw new Error('Не вдалося отримати адресу');
    }

    const data = await response.json();
    return data.display_name || `Lat: ${lat.toFixed(4)}, Lng: ${lng.toFixed(4)}`;
  } catch (error) {
    return `Lat: ${lat.toFixed(4)}, Lng: ${lng.toFixed(4)}`;
  }
}

async function createPoint(lat, lng, customAddress = null, customName = null) {
  const name = customName || prompt('Введіть назву точки:', `Точка ${pointCounter}`);
  if (!name) return null;

  modeInfo.textContent = 'Отримую адресу для точки...';
  const address = customAddress || await getAddressFromCoords(lat, lng);

  const point = createPointObject({
    id: generateId(),
    name,
    address,
    lat,
    lng
  });

  points.push(point);
  pointCounter += 1;
  saveData();
  renderPointsList();
  modeInfo.textContent = `Точку "${point.name}" додано.`;
  return point;
}

async function createWarehouse(lat, lng, customAddress = null, customName = null) {
  const name = customName || prompt('Введіть назву складу:', `Склад ${warehouseCounter}`);
  if (!name) return null;

  modeInfo.textContent = 'Отримую адресу для складу...';
  const address = customAddress || await getAddressFromCoords(lat, lng);

  const warehouse = createWarehouseObject({
    id: generateId(),
    name,
    address,
    lat,
    lng
  });

  warehouses.push(warehouse);
  warehouseCounter += 1;
  saveData();
  renderWarehousesList();
  modeInfo.textContent = `Склад "${warehouse.name}" додано.`;
  return warehouse;
}

async function ensureWarehouseByAddress(address, lat, lng) {
  const existing = warehouses.find(item => normalizeText(item.address) === normalizeText(address));
  if (existing) return existing;

  const name = `Склад ${warehouseCounter}`;
  return await createWarehouse(lat, lng, address, name);
}

async function ensurePointByAddress(address, lat, lng) {
  const existing = points.find(item => normalizeText(item.address) === normalizeText(address));
  if (existing) return existing;

  const name = `Точка ${pointCounter}`;
  return await createPoint(lat, lng, address, name);
}

function normalizeText(text) {
  return String(text || '').trim().toLowerCase();
}

function generateId() {
  return Date.now() + Math.random();
}

function createPointObject(pointData) {
  const marker = L.marker([pointData.lat, pointData.lng], { icon: pointIcon }).addTo(map);
  marker.bindPopup(`<b>${pointData.name}</b><br>${pointData.address}`);

  return {
    ...pointData,
    type: 'point',
    marker
  };
}

function createWarehouseObject(warehouseData) {
  const marker = L.marker([warehouseData.lat, warehouseData.lng], { icon: warehouseIcon }).addTo(map);
  marker.bindPopup(`<b>${warehouseData.name}</b><br>${warehouseData.address}`);

  return {
    ...warehouseData,
    type: 'warehouse',
    marker
  };
}

function addTemporaryWaypoint(lat, lng) {
  const marker = L.marker([lat, lng], { icon: waypointIcon }).addTo(map);
  const index = tempWaypointCoords.length;

  marker.bindPopup(`<b>Проміжна точка ${index + 1}</b><br>Клікни по цій точці ще раз, щоб видалити.`);

  marker.on('click', (e) => {
    L.DomEvent.stopPropagation(e);
    removeTemporaryWaypoint(index);
  });

  tempWaypointMarkers.push(marker);
  tempWaypointCoords.push({ lat, lng });
  refreshWaypointMarkers();
  updateWaypointControls();
  modeInfo.textContent = 'Проміжну точку додано. Можеш додати ще або завершити маршрут.';
}

function removeTemporaryWaypoint(index) {
  if (index < 0 || index >= tempWaypointMarkers.length) return;

  const marker = tempWaypointMarkers[index];
  if (marker) {
    map.removeLayer(marker);
  }

  tempWaypointMarkers.splice(index, 1);
  tempWaypointCoords.splice(index, 1);

  refreshWaypointMarkers();
  updateWaypointControls();
  modeInfo.textContent = 'Проміжну точку видалено.';
}

function refreshWaypointMarkers() {
  tempWaypointMarkers.forEach((marker, index) => {
    marker.off('click');

    marker.bindPopup(`<b>Проміжна точка ${index + 1}</b><br>Клікни по цій точці ще раз, щоб видалити.`);

    marker.on('click', (e) => {
      L.DomEvent.stopPropagation(e);
      removeTemporaryWaypoint(index);
    });
  });
}

function clearTemporaryWaypoints() {
  tempWaypointMarkers.forEach(marker => map.removeLayer(marker));
  tempWaypointMarkers = [];
  tempWaypointCoords = [];
  updateWaypointControls();
}

function cancelWaypointMode() {
  clearTemporaryWaypoints();
  currentMode = 'view';
  updateWaypointControls();
}

function updateWaypointControls() {
  const isWaypointMode = currentMode === 'add-waypoints';
  waypointControls.classList.toggle('hidden', !isWaypointMode);
  waypointCounterText.textContent = `Проміжних точок: ${tempWaypointCoords.length}`;
}

async function createOrUpdateRoute({
  warehouse,
  point,
  routeName,
  fuelType,
  operatorName,
  loadType,
  waypointCoords = [],
  preferExistingByPair = false
}) {
  const existingRoute = routes.find(r => r.warehouseId === warehouse.id && r.pointId === point.id);
  const routeData = await buildRoadRoute({
    warehouse,
    point,
    routeName,
    fuelType,
    operatorName,
    loadType,
    waypointCoords
  });

  if (existingRoute && preferExistingByPair) {
    map.removeLayer(existingRoute.polyline);
    const color = existingRoute.color;
    Object.assign(existingRoute, createRouteObject({
      ...routeData,
      id: existingRoute.id,
      name: routeData.name,
      color
    }));
    saveData();
    renderRoutesList();
    return existingRoute;
  }

  const route = createRouteObject(routeData);
  routes.push(route);
  saveData();
  renderRoutesList();
  return route;
}

async function updateExistingRoute(routeId, {
  warehouse,
  point,
  routeName,
  fuelType,
  operatorName,
  loadType,
  waypointCoords = []
}) {
  const existingRoute = routes.find(route => route.id === routeId);
  if (!existingRoute) {
    throw new Error('Маршрут для редагування не знайдено.');
  }

  const updatedRouteData = await buildRoadRoute({
    warehouse,
    point,
    routeName,
    fuelType,
    operatorName,
    loadType,
    waypointCoords
  });

  map.removeLayer(existingRoute.polyline);

  Object.assign(
    existingRoute,
    createRouteObject({
      ...updatedRouteData,
      id: existingRoute.id,
      name: routeName,
      color: existingRoute.color
    })
  );

  saveData();
  renderRoutesList();
  return existingRoute;
}

async function buildRoadRoute({
  warehouse,
  point,
  routeName,
  fuelType,
  operatorName,
  loadType,
  waypointCoords = []
}) {
  const orderedCoords = [
    { lat: warehouse.lat, lng: warehouse.lng },
    ...waypointCoords,
    { lat: point.lat, lng: point.lng }
  ];

  const coordString = orderedCoords
    .map(item => `${item.lng},${item.lat}`)
    .join(';');

  const routeUrl =
    `https://router.project-osrm.org/route/v1/driving/${coordString}?overview=full&geometries=geojson`;

  const response = await fetch(routeUrl);

  if (!response.ok) {
    throw new Error('Не вдалося побудувати маршрут.');
  }

  const data = await response.json();

  if (!data.routes || !data.routes.length) {
    throw new Error('Маршрут не знайдено.');
  }

  const bestRoute = data.routes[0];
  const coordinates = bestRoute.geometry.coordinates.map(([lng, lat]) => [lat, lng]);
  const distanceKm = bestRoute.distance / 1000;
  const durationMinutes = bestRoute.duration / 60;
  const priceInfo = calculateRoutePrice(distanceKm, fuelType, operatorName, loadType);

  return {
    id: generateId(),
    name: routeName,
    fromName: warehouse.name,
    toName: point.name,
    warehouseId: warehouse.id,
    pointId: point.id,
    fuelType,
    operatorName,
    operatorPrice: priceInfo.pricePerUnit,
    loadType,
    distanceKm,
    durationMinutes,
    price: priceInfo.price,
    estimatedUsage: priceInfo.estimatedUsage,
    unit: priceInfo.unit,
    geometry: coordinates,
    waypointCoords,
    waypointCount: waypointCoords.length,
    color: getNextRouteColor()
  };
}

function calculateRoutePrice(distanceKm, fuelType, operatorName, loadType) {
  const fuel = FUEL_CONFIG[fuelType];
  const load = LOAD_FACTORS[loadType];
  const pricePerUnit = getOperatorFuelPrice(operatorName, fuelType);
  const adjustedConsumption = fuel.consumption * load.multiplier;
  const estimatedUsage = (distanceKm / 100) * adjustedConsumption;
  const price = estimatedUsage * pricePerUnit;

  return {
    price,
    pricePerUnit,
    estimatedUsage,
    unit: fuel.unit.includes('кВт') ? 'кВт·год' : 'л'
  };
}

function getOperatorFuelPrice(operatorName, fuelType) {
  const operator = OPERATOR_CONFIG[operatorName];
  if (!operator) return 0;
  return operator.prices[fuelType];
}

function stopCarAnimation() {
  if (carAnimationInterval) {
    clearInterval(carAnimationInterval);
    carAnimationInterval = null;
  }

  if (carMarker) {
    map.removeLayer(carMarker);
    carMarker = null;
  }
}

function getSampledRouteCoords(routeCoords, maxSteps = 120) {
  if (!Array.isArray(routeCoords) || routeCoords.length === 0) {
    return [];
  }

  if (routeCoords.length <= maxSteps) {
    return routeCoords;
  }

  const sampled = [];
  const step = Math.ceil(routeCoords.length / maxSteps);

  for (let i = 0; i < routeCoords.length; i += step) {
    sampled.push(routeCoords[i]);
  }

  const lastCoord = routeCoords[routeCoords.length - 1];
  const lastSampled = sampled[sampled.length - 1];

  if (
    !lastSampled ||
    lastSampled[0] !== lastCoord[0] ||
    lastSampled[1] !== lastCoord[1]
  ) {
    sampled.push(lastCoord);
  }

  return sampled;
}

function animateCarAlongRoute(routeCoords) {
  stopCarAnimation();

  if (!Array.isArray(routeCoords) || routeCoords.length < 2) {
    return;
  }

  const coords = routeCoords;

  let segmentIndex = 0;
  let progress = 0;

  const speed = 0.02; 

  carMarker = L.marker(coords[0], {
    icon: L.divIcon({
      className: '',
      html: `
        <div style="
          font-size: 22px;
          transform: translate(-2px, -2px);
          filter: drop-shadow(0 2px 6px rgba(0,0,0,0.35));
          animation: bounce 0.6s infinite alternate;
        ">
        🚚
        </div>
      `,
      iconSize: [24, 24],
      iconAnchor: [12, 12]
    })
  }).addTo(map);

  carAnimationInterval = setInterval(() => {
    if (segmentIndex >= coords.length - 1) {
      clearInterval(carAnimationInterval);
      carAnimationInterval = null;
      return;
    }

    

    const start = coords[segmentIndex];
    const end = coords[segmentIndex + 1];
    const speed = 0.99;
    const angle = Math.atan2(end[1] - start[1], end[0] - start[0]);

    // плавна інтерполяція
    const lat = start[0] + (end[0] - start[0]) * progress;
    const lng = start[1] + (end[1] - start[1]) * progress;

    carMarker.setLatLng([lat, lng]);

    progress += speed;

    if (progress >= 1) {
      progress = 0;
      segmentIndex++;
    }
  }, 16); // ~60 FPS
}

function createRouteObject(routeData) {
  const polyline = L.polyline(routeData.geometry, {
    color: routeData.color || '#22c55e',
    weight: 5
  }).addTo(map);

  return {
    ...routeData,
    color: routeData.color || '#22c55e',
    polyline
  };
}

function renderPointsList() {
  pointsList.innerHTML = '';

  if (points.length === 0) {
    pointsList.innerHTML = `<div class="list-item">Ще немає точок</div>`;
    return;
  }

  points.forEach(point => {
    const item = document.createElement('div');
    item.className = 'list-item';

    if (selectedPointId === point.id) {
      item.classList.add('selected-item');
    }

    item.innerHTML = `
      <div class="route-actions">
        <button class="icon-action-btn delete-btn">✖</button>
      </div>
      <div class="list-title">${point.name}</div>
      <div class="list-subtitle">${point.address}</div>
      <div class="list-subtitle">Lat: ${point.lat.toFixed(4)}, Lng: ${point.lng.toFixed(4)}</div>
      <div class="item-actions">
        <button class="select-btn">Обрати</button>
      </div>
      <span class="badge badge-point">Постачальна точка</span>
    `;

    item.querySelector('.select-btn').addEventListener('click', () => {
      selectedPointId = point.id;
      persistSelections();
      updateSelectionText();
      renderPointsList();
      modeInfo.textContent = `Обрано точку: ${point.name}`;
    });

    item.querySelector('.delete-btn').addEventListener('click', () => {
      map.removeLayer(point.marker);
      removeRoutesByPoint(point.id);
      points = points.filter(p => p.id !== point.id);

      if (selectedPointId === point.id) {
        selectedPointId = null;
      }

      persistSelections();
      saveData();
      updateSelectionText();
      renderPointsList();
      renderRoutesList();
      modeInfo.textContent = 'Точку видалено.';
    });

    pointsList.appendChild(item);
  });
}

function renderWarehousesList() {
  warehousesList.innerHTML = '';

  if (warehouses.length === 0) {
    warehousesList.innerHTML = `<div class="list-item">Ще немає складів</div>`;
    return;
  }

  warehouses.forEach(warehouse => {
    const item = document.createElement('div');
    item.className = 'list-item';

    if (selectedWarehouseId === warehouse.id) {
      item.classList.add('selected-item');
    }

    item.innerHTML = `
      <div class="route-actions">
        <button class="icon-action-btn delete-btn">✖</button>
      </div>
      <div class="list-title">${warehouse.name}</div>
      <div class="list-subtitle">${warehouse.address}</div>
      <div class="list-subtitle">Lat: ${warehouse.lat.toFixed(4)}, Lng: ${warehouse.lng.toFixed(4)}</div>
      <div class="item-actions">
        <button class="select-btn">Обрати</button>
      </div>
      <span class="badge badge-warehouse">Склад</span>
    `;

    item.querySelector('.select-btn').addEventListener('click', () => {
      selectedWarehouseId = warehouse.id;
      persistSelections();
      updateSelectionText();
      renderWarehousesList();
      modeInfo.textContent = `Обрано склад: ${warehouse.name}`;
    });

    item.querySelector('.delete-btn').addEventListener('click', () => {
      map.removeLayer(warehouse.marker);
      removeRoutesByWarehouse(warehouse.id);
      warehouses = warehouses.filter(w => w.id !== warehouse.id);

      if (selectedWarehouseId === warehouse.id) {
        selectedWarehouseId = null;
      }

      persistSelections();
      saveData();
      updateSelectionText();
      renderWarehousesList();
      renderRoutesList();
      modeInfo.textContent = 'Склад видалено.';
    });

    warehousesList.appendChild(item);
  });
}

function renderRoutesList() {
  routesList.innerHTML = '';

  if (routes.length === 0) {
    routesList.innerHTML = `<div class="list-item">Ще немає маршрутів</div>`;
    return;
  }

  routes.forEach(route => {
    const item = document.createElement('div');
    item.className = 'list-item';
    item.style.borderColor = route.color;

    if (activeRouteId === route.id) {
      item.classList.add('selected-item');
    }

    const badgeStyle = `
      background: ${hexToRgba(route.color, 0.14)};
      color: ${route.color};
      border-color: ${hexToRgba(route.color, 0.38)};
    `;

    const waypointText = route.waypointCount
      ? ` • проміжних точок: ${route.waypointCount}`
      : '';

    item.innerHTML = `
      <div class="route-actions">
        <button class="icon-action-btn edit-btn" title="Редагувати"><i class="fa-solid fa-pen"></i></button>
        <button class="icon-action-btn delete-btn" title="Видалити">✖</button>
      </div>
      <div class="list-title">${route.name}</div>
      <div class="list-subtitle">${route.fromName} → ${route.toName}</div>
      <span class="badge" style="${badgeStyle}">Маршрут побудовано</span>
      <div class="route-color-line" style="background:${route.color};"></div>
      <div class="route-extra">${route.distanceKm.toFixed(1)} км • ${Math.round(route.durationMinutes)} хв • ${FUEL_CONFIG[route.fuelType].label}${waypointText}</div>
      <div class="route-extra">Оператор: ${route.operatorName} • ${formatNumber(route.operatorPrice)} грн за ${route.unit}</div>
      <div class="route-extra">Орієнтовна ціна: ${formatMoney(route.price)}</div>
      <div class="item-actions">
        <button class="select-btn route-view-btn">Показати</button>
      </div>
    `;

    item.querySelector('.route-view-btn').addEventListener('click', () => {
      setActiveRoute(route.id);
      map.fitBounds(route.polyline.getBounds(), { padding: [30, 30] });
      modeInfo.textContent = `Показано маршрут: ${route.name}`;
    });

    item.querySelector('.edit-btn').addEventListener('click', () => {
      openPriceModalForEdit(route);
    });

    item.querySelector('.delete-btn').addEventListener('click', () => {
      map.removeLayer(route.polyline);
      routes = routes.filter(r => r.id !== route.id);

      if (activeRouteId === route.id) {
        activeRouteId = null;
        clearRoutePrice();
        stopCarAnimation();
      }

      saveData();
      renderRoutesList();
      modeInfo.textContent = 'Маршрут видалено.';
    });

    routesList.appendChild(item);
  });
}

function setActiveRoute(routeId) {
  activeRouteId = routeId;
  const route = routes.find(item => item.id === routeId);
  if (!route) return;

  route.polyline.setStyle({ color: route.color, weight: 6, opacity: 1 });
  route.polyline.bringToFront();

  routes.forEach(item => {
    if (item.id !== routeId) {
      item.polyline.setStyle({ color: item.color, weight: 5, opacity: 0.92 });
    }
  });

  renderRoutesList();
  updateRoutePriceBox(route);
  map.fitBounds(route.polyline.getBounds(), { padding: [30, 30] });
  animateCarAlongRoute(route.geometry);
}

function updateSelectionText() {
  const selectedPoint = points.find(p => p.id === selectedPointId);
  const selectedWarehouse = warehouses.find(w => w.id === selectedWarehouseId);

  selectedPointText.textContent = selectedPoint ? selectedPoint.name : 'не обрано';
  selectedWarehouseText.textContent = selectedWarehouse ? selectedWarehouse.name : 'не обрано';
}

function updateRoutePriceBox(route) {
  const fuelLabel = FUEL_CONFIG[route.fuelType].label;
  const loadLabel = LOAD_FACTORS[route.loadType].label.toLowerCase();

  routePriceText.textContent = formatMoney(route.price);
  routePriceMeta.textContent =
    `${route.name} • ${route.distanceKm.toFixed(1)} км • ${fuelLabel}, ${route.operatorName}, ` +
    `${formatNumber(route.operatorPrice)} грн/${route.unit}, завантаженість: ${loadLabel}`;

  localStorage.setItem(STORAGE_KEYS.lastRoutePrice, JSON.stringify({
    price: route.price,
    routeName: route.name,
    distanceKm: route.distanceKm,
    fuelType: route.fuelType,
    operatorName: route.operatorName,
    operatorPrice: route.operatorPrice,
    loadType: route.loadType,
    unit: route.unit
  }));
}

function clearRoutePrice() {
  routePriceText.textContent = 'ще не розраховано';
  routePriceMeta.textContent = 'Оберіть маршрут або розрахуйте новий';
  localStorage.removeItem(STORAGE_KEYS.lastRoutePrice);
}

function restoreLastRoutePrice() {
  const raw = localStorage.getItem(STORAGE_KEYS.lastRoutePrice);
  if (!raw) return;

  try {
    const data = JSON.parse(raw);
    routePriceText.textContent = formatMoney(data.price);
    routePriceMeta.textContent =
      `${data.routeName} • ${Number(data.distanceKm).toFixed(1)} км • ` +
      `${FUEL_CONFIG[data.fuelType].label}, ${data.operatorName}, ${formatNumber(data.operatorPrice)} грн/${data.unit}`;
  } catch (error) {
    clearRoutePrice();
  }
}

function removeRoutesByPoint(pointId) {
  const activeRouteWillBeDeleted = routes.some(
    route => route.pointId === pointId && route.id === activeRouteId
  );
  
  if (activeRouteWillBeDeleted) {
    activeRouteId = null;
    clearRoutePrice();
    stopCarAnimation();
  }
  const routesToDelete = routes.filter(route => route.pointId === pointId);
}

function removeRoutesByWarehouse(warehouseId) {
  const activeRouteWillBeDeleted = routes.some(
    route => route.warehouseId === warehouseId && route.id === activeRouteId
  );
  
  if (activeRouteWillBeDeleted) {
    activeRouteId = null;
    clearRoutePrice();
    stopCarAnimation();
  }
  const routesToDelete = routes.filter(route => route.warehouseId === warehouseId);
}

function persistSelections() {
  localStorage.setItem(STORAGE_KEYS.selectedPointId, selectedPointId ?? '');
  localStorage.setItem(STORAGE_KEYS.selectedWarehouseId, selectedWarehouseId ?? '');
}

function saveData() {
  localStorage.setItem(STORAGE_KEYS.points, JSON.stringify(points.map(point => ({
    id: point.id,
    name: point.name,
    address: point.address,
    lat: point.lat,
    lng: point.lng
  }))));

  localStorage.setItem(STORAGE_KEYS.warehouses, JSON.stringify(warehouses.map(warehouse => ({
    id: warehouse.id,
    name: warehouse.name,
    address: warehouse.address,
    lat: warehouse.lat,
    lng: warehouse.lng
  }))));

  localStorage.setItem(STORAGE_KEYS.routes, JSON.stringify(routes.map(route => ({
    id: route.id,
    name: route.name,
    fromName: route.fromName,
    toName: route.toName,
    warehouseId: route.warehouseId,
    pointId: route.pointId,
    fuelType: route.fuelType,
    operatorName: route.operatorName,
    operatorPrice: route.operatorPrice,
    loadType: route.loadType,
    distanceKm: route.distanceKm,
    durationMinutes: route.durationMinutes,
    price: route.price,
    estimatedUsage: route.estimatedUsage,
    unit: route.unit,
    geometry: route.geometry,
    color: route.color,
    waypointCoords: route.waypointCoords || [],
    waypointCount: route.waypointCount || 0
  }))));

  localStorage.setItem(STORAGE_KEYS.pointCounter, String(pointCounter));
  localStorage.setItem(STORAGE_KEYS.warehouseCounter, String(warehouseCounter));
  localStorage.setItem(STORAGE_KEYS.routeCounter, String(routeCounter));
}
async function ensureJsonSeeded() {
  const alreadySeeded = localStorage.getItem(STORAGE_KEYS.jsonSeeded);
  const hasPoints = localStorage.getItem(STORAGE_KEYS.points);
  const hasWarehouses = localStorage.getItem(STORAGE_KEYS.warehouses);
  const hasRoutes = localStorage.getItem(STORAGE_KEYS.routes);

  if (alreadySeeded && hasPoints && hasWarehouses && hasRoutes) {
    return;
  }

  const pointsData = [
    {
      id: 1,
      name: 'Точка 1',
      address: 'Львів, вул. Шевченка, 10',
      lat: 49.8397,
      lng: 24.0297
    },
    {
      id: 2,
      name: 'Точка 2',
      address: 'Львів, вул. Городоцька, 120',
      lat: 49.8333,
      lng: 23.9981
    }
  ];

  const warehousesData = [
    {
      id: 1,
      name: 'Склад 1',
      address: 'Львів, вул. Наукова, 7',
      lat: 49.8078,
      lng: 24.0181
    },
    {
      id: 2,
      name: 'Склад 2',
      address: 'Львів, вул. Зелена, 147',
      lat: 49.8265,
      lng: 24.0584
    }
  ];

  const routesData = [];

  localStorage.setItem(STORAGE_KEYS.points, JSON.stringify(pointsData));
  localStorage.setItem(STORAGE_KEYS.warehouses, JSON.stringify(warehousesData));
  localStorage.setItem(STORAGE_KEYS.routes, JSON.stringify(routesData));

  const maxPointId = getMaxNumericId(pointsData);
  const maxWarehouseId = getMaxNumericId(warehousesData);
  const maxRouteId = getMaxNumericId(routesData);

  localStorage.setItem(STORAGE_KEYS.pointCounter, String(maxPointId + 1 || 1));
  localStorage.setItem(STORAGE_KEYS.warehouseCounter, String(maxWarehouseId + 1 || 1));
  localStorage.setItem(STORAGE_KEYS.routeCounter, String(maxRouteId + 1 || 1));

  localStorage.setItem(STORAGE_KEYS.jsonSeeded, 'true');
}

  function getMaxNumericId(items) {
    if (!Array.isArray(items) || !items.length) return 0;
  
    return items.reduce((max, item) => {
      const value = Number(item.id);
      return Number.isFinite(value) ? Math.max(max, value) : max;
    }, 0);
  }

  function loadSavedData() {
    try {
      const savedPoints = JSON.parse(localStorage.getItem(STORAGE_KEYS.points) || '[]');
      const savedWarehouses = JSON.parse(localStorage.getItem(STORAGE_KEYS.warehouses) || '[]');
      const savedRoutes = JSON.parse(localStorage.getItem(STORAGE_KEYS.routes) || '[]');
  
      points = savedPoints.map(createPointObject);
      warehouses = savedWarehouses.map(createWarehouseObject);
      routes = savedRoutes.map(createRouteObject);
  
      const layers = [
        ...points.map(p => p.marker),
        ...warehouses.map(w => w.marker),
        ...routes.map(r => r.polyline)
      ];
  
      if (layers.length) {
        const group = L.featureGroup(layers);
        map.fitBounds(group.getBounds(), { padding: [30, 30] });
      }
  
      setTimeout(() => {
        map.invalidateSize();
      }, 300);
    } catch (error) {
      console.error('Помилка читання localStorage:', error);
      points = [];
      warehouses = [];
      routes = [];
    }
  }
  
  function formatMoney(value) {
  return `${Number(value).toFixed(0)} грн`;
}

function formatNumber(value) {
  return Number(value).toFixed(2);
}

function getNextRouteColor() {
  return ROUTE_COLORS[(routeCounter++ - 1) % ROUTE_COLORS.length];
}

function hexToRgba(hex, alpha) {
  const sanitized = hex.replace('#', '');
  const bigint = parseInt(sanitized, 16);
  const r = (bigint >> 16) & 255;
  const g = (bigint >> 8) & 255;
  const b = bigint & 255;
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

initApp();