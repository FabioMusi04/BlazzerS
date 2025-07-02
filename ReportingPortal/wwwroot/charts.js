window.areaChart = null;
window.categoryChart = null;
window.topUsersChart = null;
window.userRegistrationChart = null;

window.renderAreaChart = (categories, data) => {
    const el = document.getElementById("area-chart");
    if (!el || typeof ApexCharts === "undefined") return;

    if (window.areaChart) {
        window.areaChart.destroy();
    }

    const options = {
        chart: {
            height: 300,
            type: "area",
            fontFamily: "Inter, sans-serif",
            toolbar: { show: false },
            dropShadow: { enabled: false }
        },
        tooltip: { enabled: true, x: { show: false } },
        dataLabels: { enabled: false },
        stroke: { width: 4, curve: 'smooth' },
        fill: {
            type: "gradient",
            gradient: {
                shadeIntensity: 0.5,
                opacityFrom: 0.8,
                opacityTo: 0.2,
                gradientToColors: ["#1A56DB"],
                stops: [0, 90, 100]
            }
        },
        series: [{
            name: "Reports",
            data: data,
            color: "#1A56DB"
        }],
        xaxis: {
            categories: categories,
            labels: { show: true },
            axisBorder: { show: true },
            axisTicks: { show: true }
        },
        yaxis: {
            show: true
        },
        grid: {
            borderColor: '#e0e0e0',
            strokeDashArray: 4
        }
    };

    window.areaChart = new ApexCharts(el, options);
    window.areaChart.render();
};

window.renderUserRegistrationChart = (categories, data) => {
    const el = document.getElementById("user-registration-chart");
    if (!el || typeof ApexCharts === "undefined") return;

    if (window.userRegistrationChart) {
        window.userRegistrationChart.destroy();
    }

    const options = {
        chart: {
            height: 300,
            type: "area",
            fontFamily: "Inter, sans-serif",
            toolbar: { show: false },
            dropShadow: { enabled: false }
        },
        tooltip: { enabled: true, x: { show: false } },
        dataLabels: { enabled: false },
        stroke: { width: 4, curve: 'smooth' },
        fill: {
            type: "gradient",
            gradient: {
                shadeIntensity: 0.5,
                opacityFrom: 0.75,
                opacityTo: 0.2,
                gradientToColors: ["#7C3AED"], // viola
                stops: [0, 90, 100]
            }
        },
        series: [{
            name: "User Registrations",
            data: data,
            color: "#7C3AED"
        }],
        xaxis: {
            categories: categories,
            labels: { show: true },
            axisBorder: { show: true },
            axisTicks: { show: true }
        },
        yaxis: {
            show: true
        },
        grid: {
            borderColor: '#e0e0e0',
            strokeDashArray: 4
        }
    };

    window.userRegistrationChart = new ApexCharts(el, options);
    window.userRegistrationChart.render();
};

window.renderReportsPerCategoryChart = (categories, counts) => {
    const el = document.getElementById("category-chart");
    if (!el || typeof ApexCharts === "undefined") return;

    if (window.categoryChart) {
        window.categoryChart.destroy();
    }

    const options = {
        chart: {
            type: 'bar',
            height: 300,
            fontFamily: "Inter, sans-serif",
            toolbar: { show: false }
        },
        series: [{
            name: 'Reports',
            data: counts
        }],
        xaxis: {
            categories: categories,
            labels: { rotate: -45, trim: true, style: { fontSize: '12px' } }
        },
        colors: ['#4F46E5'],
        dataLabels: {
            enabled: true,
            style: { fontSize: '12px' }
        },
        grid: {
            borderColor: '#e0e0e0',
            strokeDashArray: 4
        }
    };

    window.categoryChart = new ApexCharts(el, options);
    window.categoryChart.render();
};

window.renderTopUsersPieChart = (labels, counts) => {
    const el = document.getElementById("topusers-chart");
    if (!el || typeof ApexCharts === "undefined") return;

    if (window.topUsersChart) {
        window.topUsersChart.destroy();
    }

    const options = {
        chart: {
            type: 'pie',
            height: 300,
            fontFamily: "Inter, sans-serif",
            toolbar: { show: false }
        },
        series: counts,
        labels: labels,
        colors: ['#6366F1', '#818CF8', '#A5B4FC', '#C7D2FE', '#E0E7FF'],
        legend: {
            position: 'bottom',
            fontSize: '14px',
            labels: {
                colors: '#4B5563'
            }
        },
        dataLabels: {
            enabled: true,
            formatter(val, opts) {
                return `${val.toFixed(0)}%`;
            }
        }
    };

    window.topUsersChart = new ApexCharts(el, options);
    window.topUsersChart.render();
};


window.openPdfFromBase64 = (base64Data) => {
    const blob = base64ToBlob(base64Data, 'application/pdf');
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
    // Rilascia l'URL dopo un po' per liberare memoria
    setTimeout(() => URL.revokeObjectURL(url), 10000);
}

function base64ToBlob(base64, mime) {
    const byteChars = atob(base64);
    const byteNumbers = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mime });
}
