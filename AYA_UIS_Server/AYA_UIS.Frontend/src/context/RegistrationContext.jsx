// src/context/RegistrationContext.jsx
import { createContext, useContext, useState, useEffect, useCallback } from "react";
import {
  getRegistrationStatus,
  startRegistration as apiStart,
  stopRegistration as apiStop,
  updateRegistrationSettings as apiUpdate,
} from "../services/api/adminApi";

const RegistrationContext = createContext(null);

const DEFAULT_STATE = {
  isOpen:             false,
  semester:           "",
  academicYear:       "",
  startDate:          "",
  deadline:           "",
  openedCoursesByYear: {},  // { "1": ["CS101"], "2": [...], ... }
  maxCredits:         null,
};

export function RegistrationProvider({ children }) {
  const [regWindow, setRegWindow] = useState(DEFAULT_STATE);
  const [loading, setLoading]     = useState(true);
  const [error, setError]         = useState(null);

  // Fetch current state from backend on mount
  const refresh = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getRegistrationStatus();
      setRegWindow({
        isOpen:              data.isOpen ?? false,
        semester:            data.semester ?? "",
        academicYear:        data.academicYear ?? "",
        startDate:           data.startDate ?? "",
        deadline:            data.deadline ?? "",
        openedCoursesByYear: data.openedCoursesByYear ?? {},
        maxCredits:          data.maxCredits ?? null,
      });
    } catch (err) {
      // If 401/403, silently ignore (user not admin or not logged in)
      if (err?.response?.status !== 401 && err?.response?.status !== 403) {
        setError(err.message || "Failed to load registration status");
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

  /**
   * POST /api/admin/registration/start
   * @param {Object} settings - { semester, academicYear, startDate, deadline, openedCoursesByYear, maxCredits }
   */
  const startRegistration = useCallback(async (settings) => {
    const dto = {
      semester:            settings.semester,
      academicYear:        settings.academicYear,
      startDate:           settings.startDate,
      deadline:            settings.deadline,
      openedCoursesByYear: settings.openedCoursesByYear,
      maxCredits:          settings.maxCredits || null,
    };
    await apiStart(dto);
    await refresh();
  }, [refresh]);

  /**
   * POST /api/admin/registration/stop
   */
  const stopRegistration = useCallback(async () => {
    await apiStop();
    await refresh();
  }, [refresh]);

  /**
   * PUT /api/admin/registration/settings
   * @param {Object} settings - same shape as start
   */
  const updateSettings = useCallback(async (settings) => {
    const dto = {
      semester:            settings.semester,
      academicYear:        settings.academicYear,
      startDate:           settings.startDate,
      deadline:            settings.deadline,
      openedCoursesByYear: settings.openedCoursesByYear,
      maxCredits:          settings.maxCredits || null,
    };
    await apiUpdate(dto);
    await refresh();
  }, [refresh]);

  return (
    <RegistrationContext.Provider value={{
      regWindow,
      loading,
      error,
      startRegistration,
      stopRegistration,
      updateSettings,
      refresh,
    }}>
      {children}
    </RegistrationContext.Provider>
  );
}

export function useRegistration() {
  const ctx = useContext(RegistrationContext);
  if (!ctx) throw new Error("useRegistration must be inside RegistrationProvider");
  return ctx;
}
