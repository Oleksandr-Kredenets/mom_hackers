const loginTab = document.getElementById('loginTab');
const registerTab = document.getElementById('registerTab');
const submitBtn = document.getElementById('submitBtn');
const nameField = document.getElementById('nameField');
const authForm = document.getElementById('authForm');

const body = document.body;
const lightBtn = document.getElementById('lightBtn');
const darkBtn = document.getElementById('darkBtn');

let isLoginMode = true;

loginTab.addEventListener('click', () => {
  isLoginMode = true;
  loginTab.classList.add('active');
  registerTab.classList.remove('active');
  nameField.classList.add('hidden');
  submitBtn.textContent = 'Увійти';
});

registerTab.addEventListener('click', () => {
  isLoginMode = false;
  registerTab.classList.add('active');
  loginTab.classList.remove('active');
  nameField.classList.remove('hidden');
  submitBtn.textContent = 'Зареєструватися';
});

lightBtn.addEventListener('click', () => {
  body.classList.remove('dark');
  localStorage.setItem('theme', 'light');
});

darkBtn.addEventListener('click', () => {
  body.classList.add('dark');
  localStorage.setItem('theme', 'dark');
});

const savedTheme = localStorage.getItem('theme');
if (savedTheme === 'light') {
  body.classList.remove('dark');
} else {
  body.classList.add('dark');
}

authForm.addEventListener('submit', (e) => {
  e.preventDefault();

  const email = document.getElementById('email').value.trim();
  const password = document.getElementById('password').value.trim();
  const name = document.getElementById('name').value.trim();

  if (!email || !password) {
    alert('Заповніть email і пароль');
    return;
  }

  if (!isLoginMode && !name) {
    alert("Введіть ім'я");
    return;
  }

  localStorage.setItem('userEmail', email);

  if (!isLoginMode) {
    localStorage.setItem('userName', name);
  }

  window.location.href = './dashboard.html';
});