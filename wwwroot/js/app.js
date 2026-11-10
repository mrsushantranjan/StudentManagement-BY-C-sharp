(() => {
  const state = {
    page: 1,
    pageSize: 8,
    search: "",
    department: "",
    isActive: "",
    sortBy: "id",
    sortOrder: "asc",
    editingId: null,
    deletingId: null,
    totalPages: 1
  };

  const els = {
    healthBadge: document.getElementById("healthBadge"),
    healthText: document.getElementById("healthText"),
    statTotal: document.getElementById("statTotal"),
    statActive: document.getElementById("statActive"),
    statInactive: document.getElementById("statInactive"),
    statAvgGpa: document.getElementById("statAvgGpa"),
    deptChips: document.getElementById("deptChips"),
    searchInput: document.getElementById("searchInput"),
    departmentFilter: document.getElementById("departmentFilter"),
    statusFilter: document.getElementById("statusFilter"),
    sortBySelect: document.getElementById("sortBySelect"),
    btnSortOrder: document.getElementById("btnSortOrder"),
    studentsTableBody: document.getElementById("studentsTableBody"),
    telemetryText: document.getElementById("telemetryText"),
    pageInfo: document.getElementById("pageInfo"),
    btnPrevPage: document.getElementById("btnPrevPage"),
    btnNextPage: document.getElementById("btnNextPage"),
    btnAddStudent: document.getElementById("btnAddStudent"),
    studentModal: document.getElementById("studentModal"),
    modalTitle: document.getElementById("modalTitle"),
    btnCloseModal: document.getElementById("btnCloseModal"),
    btnCancelModal: document.getElementById("btnCancelModal"),
    studentForm: document.getElementById("studentForm"),
    formErrorBanner: document.getElementById("formErrorBanner"),
    activeFieldGroup: document.getElementById("activeFieldGroup"),
    firstName: document.getElementById("firstName"),
    lastName: document.getElementById("lastName"),
    email: document.getElementById("email"),
    enrollmentNumber: document.getElementById("enrollmentNumber"),
    department: document.getElementById("department"),
    semester: document.getElementById("semester"),
    gpa: document.getElementById("gpa"),
    enrollmentDate: document.getElementById("enrollmentDate"),
    isActive: document.getElementById("isActive"),
    btnSubmitStudent: document.getElementById("btnSubmitStudent"),
    deleteModal: document.getElementById("deleteModal"),
    deleteMessage: document.getElementById("deleteMessage"),
    btnCloseDelete: document.getElementById("btnCloseDelete"),
    btnCancelDelete: document.getElementById("btnCancelDelete"),
    btnConfirmDelete: document.getElementById("btnConfirmDelete"),
    toastContainer: document.getElementById("toastContainer")
  };

  async function apiFetch(url, options = {}) {
    const res = await fetch(url, {
      headers: {
        "Accept": "application/json",
        ...(options.body ? { "Content-Type": "application/json" } : {}),
        ...(options.headers || {})
      },
      ...options
    });

    const correlationId = res.headers.get("X-Correlation-ID");
    const elapsedMs = res.headers.get("X-Response-Time-Ms");
    if (correlationId || elapsedMs) {
      els.telemetryText.textContent = `Last request: ${res.status} · ${elapsedMs ?? "?"}ms · Correlation-ID: ${correlationId ?? "—"}`;
    }

    if (res.status === 204) {
      return null;
    }

    const data = await res.json().catch(() => null);
    if (!res.ok) {
      throw new ApiError(res.status, data);
    }
    return data;
  }

  class ApiError extends Error {
    constructor(status, payload) {
      const validationMsgs = payload?.errors
        ? Object.entries(payload.errors)
            .map(([field, msgs]) => `${field}: ${msgs.join(", ")}`)
            .join(" | ")
        : null;
      super(validationMsgs || payload?.detail || payload?.title || `Request failed (${status})`);
      this.status = status;
      this.payload = payload;
    }
  }

  function showToast(message, type = "info") {
    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.textContent = message;
    els.toastContainer.appendChild(toast);
    setTimeout(() => toast.remove(), 3500);
  }

  function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text ?? "";
    return div.innerHTML;
  }

  async function loadHealth() {
    try {
      const data = await apiFetch("/api/health");
      els.healthBadge.className = "health-pill healthy";
      els.healthText.textContent = `API ${data.status}`;
    } catch {
      els.healthBadge.className = "health-pill unhealthy";
      els.healthText.textContent = "API Offline";
    }
  }

  async function loadStats() {
    try {
      const stats = await apiFetch("/api/students/stats");
      els.statTotal.textContent = stats.totalStudents;
      els.statActive.textContent = stats.activeStudents;
      els.statInactive.textContent = stats.inactiveStudents;
      els.statAvgGpa.textContent = Number(stats.averageGpa).toFixed(2);
      renderDepartments(stats.studentsByDepartment || {});
    } catch (err) {
      showToast(err.message, "error");
    }
  }

  function renderDepartments(byDept) {
    const departments = Object.keys(byDept);
    const current = state.department;

    els.departmentFilter.innerHTML = `<option value="">All Departments</option>` +
      departments.map(d => `<option value="${escapeHtml(d)}"${d === current ? " selected" : ""}>${escapeHtml(d)} (${byDept[d]})</option>`).join("");

    const allActive = current === "" ? " active" : "";
    let chipsHtml = `<button class="chip${allActive}" data-dept="">All</button>`;
    for (const dept of departments) {
      const active = dept === current ? " active" : "";
      chipsHtml += `<button class="chip${active}" data-dept="${escapeHtml(dept)}">${escapeHtml(dept)} (${byDept[dept]})</button>`;
    }
    els.deptChips.innerHTML = chipsHtml;
  }

  async function loadStudents() {
    const params = new URLSearchParams({
      page: String(state.page),
      pageSize: String(state.pageSize),
      sortBy: state.sortBy,
      sortOrder: state.sortOrder
    });

    if (state.search.trim()) params.set("search", state.search.trim());
    if (state.department) params.set("department", state.department);
    if (state.isActive !== "") params.set("isActive", state.isActive);

    try {
      const paged = await apiFetch(`/api/students?${params.toString()}`);
      state.totalPages = Math.max(1, paged.totalPages);
      renderStudentsTable(paged.items);
      els.pageInfo.textContent = `Page ${paged.page} of ${state.totalPages} (${paged.totalCount} total)`;
      els.btnPrevPage.disabled = !paged.hasPrevious;
      els.btnNextPage.disabled = !paged.hasNext;
    } catch (err) {
      els.studentsTableBody.innerHTML = `<tr><td colspan="7" class="empty-state">${escapeHtml(err.message)}</td></tr>`;
      showToast(err.message, "error");
    }
  }

  function renderStudentsTable(items) {
    if (!items || items.length === 0) {
      els.studentsTableBody.innerHTML = `<tr><td colspan="7" class="empty-state">No students match the current filters.</td></tr>`;
      return;
    }

    els.studentsTableBody.innerHTML = items.map(s => {
      const initials = `${(s.firstName?.[0] || "").toUpperCase()}${(s.lastName?.[0] || "").toUpperCase()}`;
      const gpaPct = Math.min(100, Math.max(0, (Number(s.gpa) / 10) * 100));
      const statusClass = s.isActive ? "status-active" : "status-inactive";
      const statusLabel = s.isActive ? "Active" : "Inactive";

      return `
        <tr>
          <td>
            <div class="student-cell">
              <div class="avatar">${escapeHtml(initials)}</div>
              <div>
                <div class="student-name">${escapeHtml(s.fullName)}</div>
                <div class="student-email">${escapeHtml(s.email)}</div>
              </div>
            </div>
          </td>
          <td><span class="mono-badge">${escapeHtml(s.enrollmentNumber)}</span></td>
          <td><span class="dept-pill">${escapeHtml(s.department)}</span></td>
          <td>Sem ${escapeHtml(String(s.semester))}</td>
          <td>
            <div class="gpa-wrap">
              <strong>${Number(s.gpa).toFixed(2)}</strong>
              <div class="gpa-bar"><div class="gpa-fill" style="width:${gpaPct}%"></div></div>
            </div>
          </td>
          <td>
            <button class="status-btn ${statusClass}" data-action="toggle" data-id="${s.id}" data-active="${s.isActive}" title="Click to toggle status">
              ${statusLabel}
            </button>
          </td>
          <td class="text-right">
            <div class="row-actions">
              <button class="btn btn-outline btn-sm" data-action="edit" data-id="${s.id}">Edit</button>
              <button class="btn btn-ghost btn-sm" data-action="delete" data-id="${s.id}" data-name="${escapeHtml(s.fullName)}">Delete</button>
            </div>
          </td>
        </tr>`;
    }).join("");
  }

  function openStudentModal(student = null) {
    els.formErrorBanner.classList.add("hidden");
    els.formErrorBanner.textContent = "";

    if (student) {
      state.editingId = student.id;
      els.modalTitle.textContent = `Edit Student #${student.id}`;
      els.btnSubmitStudent.textContent = "Update Student";
      els.firstName.value = student.firstName;
      els.lastName.value = student.lastName;
      els.email.value = student.email;
      els.enrollmentNumber.value = student.enrollmentNumber;
      els.department.value = student.department;
      els.semester.value = student.semester;
      els.gpa.value = student.gpa;
      els.enrollmentDate.value = student.enrollmentDate || "";
      els.isActive.checked = Boolean(student.isActive);
      els.activeFieldGroup.classList.remove("hidden");
    } else {
      state.editingId = null;
      els.modalTitle.textContent = "Add Student";
      els.btnSubmitStudent.textContent = "Create Student";
      els.studentForm.reset();
      els.semester.value = 1;
      els.gpa.value = "8.50";
      els.enrollmentDate.value = new Date().toISOString().slice(0, 10);
      els.isActive.checked = true;
      els.activeFieldGroup.classList.add("hidden");
    }

    els.studentModal.classList.remove("hidden");
    els.firstName.focus();
  }

  function closeStudentModal() {
    els.studentModal.classList.add("hidden");
    state.editingId = null;
  }

  function openDeleteModal(id, name) {
    state.deletingId = id;
    els.deleteMessage.textContent = `Delete "${name}"? This action cannot be undone.`;
    els.deleteModal.classList.remove("hidden");
  }

  function closeDeleteModal() {
    els.deleteModal.classList.add("hidden");
    state.deletingId = null;
  }

  async function refreshAll() {
    await Promise.all([loadStats(), loadStudents()]);
  }

  // Event Listeners
  let searchTimer;
  els.searchInput.addEventListener("input", () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
      state.search = els.searchInput.value;
      state.page = 1;
      loadStudents();
    }, 220);
  });

  els.departmentFilter.addEventListener("change", () => {
    state.department = els.departmentFilter.value;
    state.page = 1;
    refreshAll();
  });

  els.deptChips.addEventListener("click", (e) => {
    const chip = e.target.closest("[data-dept]");
    if (!chip) return;
    state.department = chip.dataset.dept;
    state.page = 1;
    refreshAll();
  });

  els.statusFilter.addEventListener("change", () => {
    state.isActive = els.statusFilter.value;
    state.page = 1;
    loadStudents();
  });

  els.sortBySelect.addEventListener("change", () => {
    state.sortBy = els.sortBySelect.value;
    state.page = 1;
    loadStudents();
  });

  els.btnSortOrder.addEventListener("click", () => {
    state.sortOrder = state.sortOrder === "asc" ? "desc" : "asc";
    els.btnSortOrder.textContent = state.sortOrder === "asc" ? "↑ Asc" : "↓ Desc";
    state.page = 1;
    loadStudents();
  });

  els.btnPrevPage.addEventListener("click", () => {
    if (state.page > 1) {
      state.page--;
      loadStudents();
    }
  });

  els.btnNextPage.addEventListener("click", () => {
    if (state.page < state.totalPages) {
      state.page++;
      loadStudents();
    }
  });

  els.btnAddStudent.addEventListener("click", () => openStudentModal(null));
  els.btnCloseModal.addEventListener("click", closeStudentModal);
  els.btnCancelModal.addEventListener("click", closeStudentModal);

  els.btnCloseDelete.addEventListener("click", closeDeleteModal);
  els.btnCancelDelete.addEventListener("click", closeDeleteModal);

  els.studentsTableBody.addEventListener("click", async (e) => {
    const btn = e.target.closest("[data-action]");
    if (!btn) return;
    const action = btn.dataset.action;
    const id = Number(btn.dataset.id);

    if (action === "toggle") {
      const nextActive = btn.dataset.active !== "true";
      try {
        await apiFetch(`/api/students/${id}/status`, {
          method: "PATCH",
          body: JSON.stringify({ isActive: nextActive })
        });
        showToast(`Student marked ${nextActive ? "Active" : "Inactive"}`, "success");
        await refreshAll();
      } catch (err) {
        showToast(err.message, "error");
      }
    } else if (action === "edit") {
      try {
        const student = await apiFetch(`/api/students/${id}`);
        openStudentModal(student);
      } catch (err) {
        showToast(err.message, "error");
      }
    } else if (action === "delete") {
      openDeleteModal(id, btn.dataset.name);
    }
  });

  els.studentForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    els.formErrorBanner.classList.add("hidden");

    const payload = {
      firstName: els.firstName.value.trim(),
      lastName: els.lastName.value.trim(),
      email: els.email.value.trim(),
      enrollmentNumber: els.enrollmentNumber.value.trim(),
      department: els.department.value.trim(),
      semester: Number(els.semester.value),
      gpa: Number(els.gpa.value),
      enrollmentDate: els.enrollmentDate.value || null
    };

    try {
      els.btnSubmitStudent.disabled = true;
      if (state.editingId) {
        await apiFetch(`/api/students/${state.editingId}`, {
          method: "PUT",
          body: JSON.stringify({
            ...payload,
            enrollmentDate: payload.enrollmentDate || new Date().toISOString().slice(0, 10),
            isActive: els.isActive.checked
          })
        });
        showToast("Student updated successfully", "success");
      } else {
        await apiFetch("/api/students", {
          method: "POST",
          body: JSON.stringify(payload)
        });
        showToast("Student created successfully", "success");
      }
      closeStudentModal();
      await refreshAll();
    } catch (err) {
      els.formErrorBanner.textContent = err.message;
      els.formErrorBanner.classList.remove("hidden");
    } finally {
      els.btnSubmitStudent.disabled = false;
    }
  });

  els.btnConfirmDelete.addEventListener("click", async () => {
    if (!state.deletingId) return;
    try {
      els.btnConfirmDelete.disabled = true;
      await apiFetch(`/api/students/${state.deletingId}`, { method: "DELETE" });
      showToast("Student deleted", "success");
      closeDeleteModal();
      await refreshAll();
    } catch (err) {
      showToast(err.message, "error");
    } finally {
      els.btnConfirmDelete.disabled = false;
    }
  });

  // Close modals on Escape
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      closeStudentModal();
      closeDeleteModal();
    }
  });

  // Initial load
  loadHealth();
  refreshAll();
})();

