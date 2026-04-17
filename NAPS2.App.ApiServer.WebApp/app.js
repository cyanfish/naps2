const apiUrlInput = document.getElementById('apiUrl');
const statusOutput = document.getElementById('statusOutput');
const scanOutput = document.getElementById('scanOutput');
const logOutput = document.getElementById('logOutput');
const driverSelect = document.getElementById('driverSelect');
const deviceList = document.getElementById('deviceList');
const deviceCaps = document.getElementById('deviceCaps');
const selectedDeviceInput = document.getElementById('selectedDevice');
const btnRefreshDevices = document.getElementById('btnRefreshDevices');
const btnHealth = document.getElementById('btnHealth');
const btnStatus = document.getElementById('btnStatus');
const btnVersion = document.getElementById('btnVersion');
const btnSettings = document.getElementById('btnSettings');
const btnStartScan = document.getElementById('btnStartScan');
const btnRefreshJobs = document.getElementById('btnRefreshJobs');
const jobsList = document.getElementById('jobsList');
const jobDetails = document.getElementById('jobDetails');
const paperSourceSelect = document.getElementById('paperSourceSelect');
const pageSizeSelect = document.getElementById('pageSizeSelect');
const colorModeSelect = document.getElementById('colorModeSelect');
const dpiInput = document.getElementById('dpiInput');
const qualityInput = document.getElementById('qualityInput');

let selectedDevice = null;
let currentJobs = [];

function formatJson(value) {
  try {
    return JSON.stringify(value, null, 2);
  } catch {
    return String(value);
  }
}

function log(message) {
  logOutput.textContent = `${new Date().toLocaleTimeString()} - ${message}\n${logOutput.textContent}`;
}

function apiBase() {
  const raw = apiUrlInput.value.trim();
  if (!raw) {
    throw new Error('请输入 API 地址');
  }
  return raw.replace(/\/+$/, '');
}

async function apiFetch(path, method = 'GET', body = null, query = {}) {
  const base = apiBase();
  const url = new URL(`${base}${path}`);
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      url.searchParams.set(key, String(value));
    }
  });

  const init = { method, headers: { 'Accept': 'application/json' } };
  if (body !== null) {
    init.headers['Content-Type'] = 'application/json';
    init.body = JSON.stringify(body);
  }

  const response = await fetch(url.toString(), init);
  const contentType = response.headers.get('content-type') || '';
  if (contentType.includes('application/json')) {
    const data = await response.json();
    if (!response.ok) {
      throw new Error(data?.message || `${response.status} ${response.statusText}`);
    }
    return data;
  }

  return response;
}

async function loadDrivers() {
  try {
    const data = await apiFetch('/scan/drivers');
    driverSelect.innerHTML = data.drivers.map(d => `<option value="${d}">${d}</option>`).join('');
    log('已加载驱动程序列表。');
    await loadDevices();
  } catch (error) {
    statusOutput.textContent = `加载驱动失败：${error.message}`;
    log(`加载驱动失败：${error.message}`);
  }
}

async function loadDevices() {
  try {
    const driver = driverSelect.value;
    const data = await apiFetch('/scan/devices', 'GET', null, { driver });
    renderDeviceTable(data.devices || []);
    statusOutput.textContent = `已加载 ${data.devices.length} 个设备。`;
    log(`设备加载完成，驱动：${driver}，设备数：${data.devices.length}`);
  } catch (error) {
    statusOutput.textContent = `加载设备失败：${error.message}`;
    log(`加载设备失败：${error.message}`);
    deviceList.innerHTML = '';
    deviceCaps.textContent = '';
  }
}

function renderDeviceTable(devices) {
  if (!devices.length) {
    deviceList.innerHTML = '<p>当前未发现设备。</p>';
    return;
  }

  const rows = devices.map((device, index) => `
      <tr>
        <td>${index + 1}</td>
        <td>${device.driver}</td>
        <td>${device.name}</td>
        <td>${device.id}</td>
        <td><button data-id="${encodeURIComponent(device.id)}" data-name="${encodeURIComponent(device.name)}" data-driver="${device.driver}">选择</button></td>
      </tr>
    `).join('');

  deviceList.innerHTML = `
    <table>
      <thead><tr><th>#</th><th>驱动</th><th>设备名称</th><th>设备 ID</th><th>操作</th></tr></thead>
      <tbody>${rows}</tbody>
    </table>
  `;

  deviceList.querySelectorAll('button').forEach(btn => {
    btn.addEventListener('click', () => {
      const device = {
        driver: btn.dataset.driver,
        id: decodeURIComponent(btn.dataset.id),
        name: decodeURIComponent(btn.dataset.name)
      };
      selectDevice(device);
    });
  });
}

async function selectDevice(device) {
  selectedDevice = device;
  selectedDeviceInput.value = `${device.driver} / ${device.name}`;
  deviceCaps.textContent = '正在查询设备能力...';
  try {
    const data = await apiFetch('/scan/caps', 'GET', null, {
      driver: device.driver,
      deviceId: device.id,
      deviceName: device.name
    });
    deviceCaps.innerHTML = `<h3>设备能力</h3><pre>${formatJson(data.caps)}</pre>`;
    log(`已选择设备：${device.name}（${device.id}）`);
  } catch (error) {
    deviceCaps.textContent = `查询能力失败：${error.message}`;
    log(`查询能力失败：${error.message}`);
  }
}

async function startScan() {
  if (!selectedDevice) {
    scanOutput.textContent = '请先选择一个设备。';
    return;
  }

  const options = {
    driver: selectedDevice.driver,
    device: selectedDevice,
    paperSource: paperSourceSelect.value,
    pageSize: pageSizeSelect.value,
    colorMode: colorModeSelect.value,
    dpi: Number(dpiInput.value) || 300,
    quality: Number(qualityInput.value) || 85
  };

  try {
    const data = await apiFetch('/scan/start', 'POST', options);
    scanOutput.textContent = formatJson(data);
    log(`扫描作业已启动：${data.jobId}`);
    await refreshJobs();
  } catch (error) {
    scanOutput.textContent = `启动扫描失败：${error.message}`;
    log(`启动扫描失败：${error.message}`);
  }
}

async function refreshJobs() {
  try {
    const data = await apiFetch('/scan/jobs');
    currentJobs = data.jobs || [];
    renderJobs(currentJobs);
    log(`已刷新作业列表 (${currentJobs.length})`);
  } catch (error) {
    log(`刷新作业失败：${error.message}`);
    jobsList.innerHTML = `<p>刷新作业失败：${error.message}</p>`;
  }
}

function renderJobs(jobs) {
  if (!jobs.length) {
    jobsList.innerHTML = '<p>当前没有扫描作业。</p>';
    jobDetails.textContent = '';
    return;
  }

  const rows = jobs.map(job => `
      <tr>
        <td>${job.jobId}</td>
        <td>${job.status}</td>
        <td>${job.startedAt || ''}</td>
        <td>${job.completedAt || ''}</td>
        <td>${job.pagesScanned || 0}</td>
        <td>
          <button data-jobid="${job.jobId}" class="details">详情</button>
          <button data-jobid="${job.jobId}" class="cancel">取消</button>
          <button data-jobid="${job.jobId}" class="export">导出 PDF</button>
        </td>
      </tr>
    `).join('');

  jobsList.innerHTML = `
    <table>
      <thead><tr><th>作业 ID</th><th>状态</th><th>开始时间</th><th>完成时间</th><th>页数</th><th>操作</th></tr></thead>
      <tbody>${rows}</tbody>
    </table>
  `;

  jobsList.querySelectorAll('button.details').forEach(btn => {
    btn.addEventListener('click', () => showJobDetails(btn.dataset.jobid));
  });

  jobsList.querySelectorAll('button.cancel').forEach(btn => {
    btn.addEventListener('click', () => cancelJob(btn.dataset.jobid));
  });

  jobsList.querySelectorAll('button.export').forEach(btn => {
    btn.addEventListener('click', () => exportJobPdf(btn.dataset.jobid));
  });
}

async function showJobDetails(jobId) {
  if (!jobId) return;
  try {
    const data = await apiFetch(`/scan/jobs/${encodeURIComponent(jobId)}`);
    jobDetails.innerHTML = `
      <h3>作业详情</h3>
      <pre>${formatJson(data)}</pre>
    `;
    log(`加载作业详情：${jobId}`);
  } catch (error) {
    jobDetails.textContent = `查询作业详情失败：${error.message}`;
    log(`查询作业详情失败：${error.message}`);
  }
}

async function cancelJob(jobId) {
  if (!jobId) return;
  try {
    const data = await apiFetch('/scan/cancel', 'POST', null, { jobId });
    log(`取消请求已发送：${jobId}，状态：${data.status}`);
    await refreshJobs();
  } catch (error) {
    log(`取消失败：${error.message}`);
  }
}

async function exportJobPdf(jobId) {
  if (!jobId) return;
  try {
    const response = await apiFetch(`/scan/jobs/${encodeURIComponent(jobId)}/export`, 'GET', null, { format: 'pdf', encoding: 'base64' });
    if (response.success && response.data) {
      const blob = base64ToBlob(response.data, response.contentType || 'application/pdf');
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `${jobId}.pdf`;
      link.click();
      log(`已导出 PDF：${jobId}`);
    } else {
      log(`导出失败：${response.message || '未知错误'}`);
    }
  } catch (error) {
    log(`导出失败：${error.message}`);
  }
}

function base64ToBlob(base64, mime) {
  const bytes = atob(base64);
  const buffer = new Uint8Array(bytes.length);
  for (let i = 0; i < bytes.length; i += 1) {
    buffer[i] = bytes.charCodeAt(i);
  }
  return new Blob([buffer], { type: mime });
}

async function fetchStatus(path) {
  try {
    const data = await apiFetch(path);
    statusOutput.textContent = formatJson(data);
    log(`已获取 ${path} 信息`);
  } catch (error) {
    statusOutput.textContent = `请求失败：${error.message}`;
    log(`请求失败：${path} - ${error.message}`);
  }
}

btnHealth.addEventListener('click', () => fetchStatus('/health'));
btnStatus.addEventListener('click', () => fetchStatus('/status'));
btnVersion.addEventListener('click', () => fetchStatus('/version'));
btnSettings.addEventListener('click', () => fetchStatus('/settings'));
btnRefreshDevices.addEventListener('click', loadDevices);
btnStartScan.addEventListener('click', startScan);
btnRefreshJobs.addEventListener('click', refreshJobs);

window.addEventListener('load', async () => {
  try {
    await loadDrivers();
    await refreshJobs();
  } catch (error) {
    log(`初始化失败：${error.message}`);
  }
});
