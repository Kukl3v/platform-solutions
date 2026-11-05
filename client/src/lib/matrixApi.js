const API_URL = import.meta.env.SERVER_API_URL ??'http://localhost:8080';

async function request(url, options = {}) {
    const response = await fetch(`${API_URL}${url}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options.headers
        }
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const error = new Error(data?.detail ?? 'Ошибка запроса');

        error.status = response.status;
        error.errors = data?.errors ?? [];

        throw error;
    }

    return data;
}

export function generateMatrix(params) {
    const query = new URLSearchParams(params);

    return request(`/api/matrix?${query}`);
}

export function calculateAverage(matrix) {
    return request('/api/matrix/calculate/average', {
        method: 'POST',
        body: JSON.stringify({ matrix })
    });
}

export function getFieldErrors(error) {
    return Object.fromEntries(
        (error.errors ?? []).map(({ field, message }) => [
            field,
            message
        ])
    );
}
