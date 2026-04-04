// захист сторінки: якщо користувач не увійшов, кидає на login
const savedUser = localStorage.getItem('userEmail');

if (!savedUser) {
  window.location.href = './login.html';
}

// тема
const body = document.body;
const lightBtn = document.getElementById('lightBtn');
const darkBtn = document.getElementById('darkBtn');

if (lightBtn) {
  lightBtn.addEventListener('click', () => {
    body.classList.remove('dark');
    localStorage.setItem('theme', 'light');
  });
}

if (darkBtn) {
  darkBtn.addEventListener('click', () => {
    body.classList.add('dark');
    localStorage.setItem('theme', 'dark');
  });
}

const savedTheme = localStorage.getItem('theme');
if (savedTheme === 'light') {
  body.classList.remove('dark');
} else {
  body.classList.add('dark');
}

// кнопка вийти
const logoutBtn = document.getElementById('logoutBtn');

if (logoutBtn) {
  logoutBtn.addEventListener('click', () => {
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userName');
    window.location.href = './login.html';
  });
}

// карта
const map = L.map('map').setView([49.84, 24.03], 13);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
  attribution: '&copy; OpenStreetMap'
}).addTo(map);

let markers = [];

map.on('click', function (e) {
  const marker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(map);

  marker.on('click', function () {
    map.removeLayer(marker);
    markers = markers.filter(m => m !== marker);
  });

  markers.push(marker);
});