// FUNews Management System v2 - Global Site Scripts
const API_BASE_URL = window.__API_BASE_URL || (window.location.protocol === 'https:' ? 'https://localhost:7082/api/' : 'http://localhost:5132/api/');

function getJwtToken() {
    const name = "jwt_token=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i].trim();
        if (c.indexOf(name) === 0) {
            return c.substring(name.length, c.length);
        }
    }
    return localStorage.getItem('jwt_token') || '';
}

function setJwtToken(token) {
    localStorage.setItem('jwt_token', token);
    const isHttps = window.location.protocol === 'https:';
    document.cookie = `jwt_token=${token}; path=/; max-age=7200; SameSite=Lax${isHttps ? '; Secure' : ''}`;
}

async function apiFetch(endpoint, options = {}) {
    if (typeof authenticatedFetch === "function") {
        const fullUrl = endpoint.startsWith('http') ? endpoint : `${window.CORE_API_URL || API_BASE_URL}${endpoint.replace(/^\//, '')}`;
        return await authenticatedFetch(fullUrl, options);
    }

    const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint.replace(/^\//, '')}`;
    options.headers = options.headers || {};
    const token = getJwtToken();
    if (token && !options.headers['Authorization']) {
        options.headers['Authorization'] = `Bearer ${token}`;
    }

    if (options.body && !(options.body instanceof FormData) && !options.headers['Content-Type']) {
        options.headers['Content-Type'] = 'application/json';
    }

    try {
        const response = await fetch(url, options);

        if (response.status === 401) {
            showToast('Session expired or unauthorized. Please sign in.', 'danger');
            setTimeout(() => {
                window.location.href = '/Login';
            }, 1500);
            throw new Error('Unauthorized');
        }

        if (response.status === 403) {
            showToast('Permission denied (403): You are not authorized for this operation.', 'danger');
            throw new Error('Forbidden');
        }

        return response;
    } catch (err) {
        console.error('API Fetch Error:', err);
        throw err;
    }
}

function showToast(message, type = 'success') {
    const toastEl = document.getElementById('appToast');
    const toastMsg = document.getElementById('toastMessage');
    const toastIcon = document.getElementById('toastIcon');

    if (!toastEl) {
        alert(message);
        return;
    }

    toastEl.className = 'toast align-items-center text-white border-0 shadow-lg';
    
    if (type === 'success' || type === true) {
        toastEl.classList.add('bg-success');
        toastIcon.className = 'bi bi-check-circle-fill fs-5';
    } else if (type === 'danger' || type === 'error' || type === false) {
        toastEl.classList.add('bg-danger');
        toastIcon.className = 'bi bi-x-circle-fill fs-5';
    } else if (type === 'warning') {
        toastEl.classList.add('bg-warning', 'text-dark');
        toastIcon.className = 'bi bi-exclamation-triangle-fill fs-5';
    } else {
        toastEl.classList.add('bg-primary');
        toastIcon.className = 'bi bi-info-circle-fill fs-5';
    }

    toastMsg.textContent = message;
    const toast = bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 4000 });
    toast.show();
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    try {
        const d = new Date(dateString);
        return d.toLocaleDateString('en-GB', {
            year: 'numeric',
            month: 'short',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch {
        return dateString;
    }
}
