const apiBase = '/api/auth';

// Auth state
let currentUser = null;
let authToken = localStorage.getItem('authToken');

// Elements
const authElements = {
  loginModal: document.getElementById('login-modal'),
  loginForm: document.getElementById('login-form'),
  loginEmail: document.getElementById('login-email'),
  loginPassword: document.getElementById('login-password'),
  loginMessage: document.getElementById('login-message'),
  registerModal: document.getElementById('register-modal'),
  registerForm: document.getElementById('register-form'),
  registerEmail: document.getElementById('register-email'),
  registerUserName: document.getElementById('register-username'),
  registerPassword: document.getElementById('register-password'),
  registerMessage: document.getElementById('register-message'),
  logoutBtn: document.getElementById('logout-btn'),
  profileSection: document.getElementById('profile-section'),
  authLinks: document.getElementById('auth-links'),
};

// Toggle auth views
function showLogin() {
  authElements.loginModal.classList.remove('hidden');
  authElements.registerModal.classList.add('hidden');
  authElements.loginForm.reset();
  authElements.loginMessage.textContent = '';
}

function showRegister() {
  authElements.registerModal.classList.remove('hidden');
  authElements.loginModal.classList.add('hidden');
  authElements.registerForm.reset();
  authElements.registerMessage.textContent = '';
}

function closeAuth() {
  authElements.loginModal.classList.add('hidden');
  authElements.registerModal.classList.add('hidden');
}

// Auth functions
async function login() {
  const email = authElements.loginEmail.value.trim();
  const password = authElements.loginPassword.value;

  if (!email || !password) {
    authElements.loginMessage.textContent = 'الرجاء إدخال البريد الإلكتروني وكلمة المرور';
    authElements.loginMessage.className = 'form-message error';
    return;
  }

  try {
    const response = await fetch(`${apiBase}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    const data = await response.json();

    if (!response.ok) {
      authElements.loginMessage.textContent = data.message || 'فشل تسجيل الدخول';
      authElements.loginMessage.className = 'form-message error';
      return;
    }

    authToken = data;
    localStorage.setItem('authToken', data);
    currentUser = { email };
    updateAuthUI();
    closeAuth();
  } catch (error) {
    authElements.loginMessage.textContent = 'حدث خطأ أثناء تسجيل الدخول';
    authElements.loginMessage.className = 'form-message error';
  }
}

async function register() {
  const email = authElements.registerEmail.value.trim();
  const userName = authElements.registerUserName.value.trim();
  const password = authElements.registerPassword.value;

  if (!email || !userName || !password) {
    authElements.registerMessage.textContent = 'الرجاء ملء جميع الحقول';
    authElements.registerMessage.className = 'form-message error';
    return;
  }

  try {
    const response = await fetch(`${apiBase}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, userName, password }),
    });

    const data = await response.json();

    if (!response.ok) {
      authElements.registerMessage.textContent = data.message || 'فشل التسجيل';
      authElements.registerMessage.className = 'form-message error';
      return;
    }

    authElements.registerMessage.textContent = 'تم التسجيل بنجاح! يمكنك تسجيل الدخول الآن';
    authElements.registerMessage.className = 'form-message success';
    setTimeout(showLogin, 1500);
  } catch (error) {
    authElements.registerMessage.textContent = 'حدث خطأ أثناء التسجيل';
    authElements.registerMessage.className = 'form-message error';
  }
}

function logout() {
  authToken = null;
  localStorage.removeItem('authToken');
  currentUser = null;
  updateAuthUI();
}

function updateAuthUI() {
  if (authToken && currentUser) {
    authElements.authLinks.classList.add('hidden');
    authElements.profileSection.classList.remove('hidden');
    document.getElementById('profile-email').textContent = currentUser.email;
  } else {
    authElements.authLinks.classList.remove('hidden');
    authElements.profileSection.classList.add('hidden');
  }
}

// Bind events
authElements.loginForm.addEventListener('submit', (e) => {
  e.preventDefault();
  login();
});

authElements.registerForm.addEventListener('submit', (e) => {
  e.preventDefault();
  register();
});

if (authElements.logoutBtn) {
  authElements.logoutBtn.addEventListener('click', logout);
}

// Initialize
if (authToken) {
  currentUser = { email: 'User' }; // Could decode JWT here
  updateAuthUI();
} else {
  updateAuthUI();
}
