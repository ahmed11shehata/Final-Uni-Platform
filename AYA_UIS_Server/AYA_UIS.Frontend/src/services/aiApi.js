// src/services/aiApi.js
// AI Study Assistant API Service — with timeouts + cancellation support

const BASE_URL = import.meta.env.VITE_AI_API_URL || 'http://localhost:8000';
export const AI_API_URL = BASE_URL;

// ── Supported file types ──────────────────────────────
export const SUPPORTED_TYPES = {
  'application/pdf': '.pdf',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document': '.docx',
  'application/vnd.openxmlformats-officedocument.presentationml.presentation': '.pptx',
  'text/plain': '.txt',
  'image/png': '.png',
  'image/jpeg': '.jpg',
  'image/jpg': '.jpg',
};
export const ACCEPT_STRING = Object.keys(SUPPORTED_TYPES).join(',');
export const MAX_FILE_SIZE_MB = 50;

// ── Timeouts (ms) ─────────────────────────────────────
const TIMEOUTS = {
  health:      5_000,
  extract:   120_000,   // 2 min  — large files can be slow
  summary:    90_000,   // 1.5 min
  quiz:       90_000,
  mindmap:    90_000,
  qbank:      90_000,
  generateAll: 180_000, // 3 min  — does everything at once
  chat:        60_000,  // 1 min per message
};

// ── Human-readable error ──────────────────────────────
function friendlyError(err, label) {
  if (err?.name === 'AbortError' || err?.name === 'TimeoutError') {
    return `${label} timed out — the server took too long to respond. Try a smaller file or check the API server.`;
  }
  if (err?.message?.includes('Failed to fetch') || err?.message?.includes('NetworkError')) {
    return 'Cannot reach the API server. Make sure it is running on port 8000.';
  }
  return err?.message || `${label} failed`;
}

// ── Response helper ───────────────────────────────────
async function handleResponse(res, label = 'Request') {
  if (!res.ok) {
    let msg = `HTTP ${res.status}`;
    try { const d = await res.json(); msg = d.error || d.detail || msg; } catch {}
    throw new Error(msg);
  }
  return res.json();
}

// ── Fetch with timeout helper ─────────────────────────
async function fetchWithTimeout(url, options = {}, timeoutMs, abortController = null) {
  // Merge external abort controller + internal timeout controller
  const timeoutCtrl = new AbortController();
  const timer = setTimeout(() => timeoutCtrl.abort(new DOMException('Timed out', 'TimeoutError')), timeoutMs);

  // If caller passes an external controller, watch it too
  let cleanupExternal = null;
  if (abortController) {
    cleanupExternal = () => timeoutCtrl.abort();
    abortController.signal.addEventListener('abort', cleanupExternal);
  }

  try {
    const res = await fetch(url, {
      ...options,
      signal: timeoutCtrl.signal,
    });
    return res;
  } finally {
    clearTimeout(timer);
    if (abortController && cleanupExternal) {
      abortController.signal.removeEventListener('abort', cleanupExternal);
    }
  }
}

// ── API Methods ───────────────────────────────────────
export const aiApi = {

  /** Check if API is running (fast, 5s timeout) */
  async health() {
    try {
      const res = await fetchWithTimeout(`${BASE_URL}/health`, {}, TIMEOUTS.health);
      return res.ok ? res.json() : null;
    } catch {
      return null;
    }
  },

  /** Extract text from uploaded file
   *  @param {File}            file
   *  @param {AbortController} [abortCtrl]  — pass to allow cancellation
   */
  async extractText(file, abortCtrl) {
    const fd = new FormData();
    fd.append('file', file);
    try {
      const res = await fetchWithTimeout(
        `${BASE_URL}/extract`,
        { method: 'POST', body: fd },
        TIMEOUTS.extract,
        abortCtrl,
      );
      return handleResponse(res, 'Text extraction');
    } catch (err) {
      throw new Error(friendlyError(err, 'Text extraction'));
    }
  },

  /** Generate summary from text
   *  @param {string}          text
   *  @param {string}          language
   *  @param {AbortController} [abortCtrl]
   */
  async generateSummary(text, language = 'en', abortCtrl) {
    const fd = new FormData();
    fd.append('text', text);
    fd.append('language', language);
    try {
      const res = await fetchWithTimeout(
        `${BASE_URL}/summary`,
        { method: 'POST', body: fd },
        TIMEOUTS.summary,
        abortCtrl,
      );
      return handleResponse(res, 'Summary generation');
    } catch (err) {
      throw new Error(friendlyError(err, 'Summary generation'));
    }
  },

  /** Generate MCQ quiz from text */
  async generateQuiz(text, numQuestions = 10, language = 'en', abortCtrl) {
    const fd = new FormData();
    fd.append('text', text);
    fd.append('num_questions', String(numQuestions));
    fd.append('language', language);
    try {
      const res = await fetchWithTimeout(
        `${BASE_URL}/quiz`,
        { method: 'POST', body: fd },
        TIMEOUTS.quiz,
        abortCtrl,
      );
      return handleResponse(res, 'Quiz generation');
    } catch (err) {
      throw new Error(friendlyError(err, 'Quiz generation'));
    }
  },

  /** Generate mind map from text */
  async generateMindmap(text, language = 'en', abortCtrl) {
    const fd = new FormData();
    fd.append('text', text);
    fd.append('language', language);
    try {
      const res = await fetchWithTimeout(
        `${BASE_URL}/mindmap`,
        { method: 'POST', body: fd },
        TIMEOUTS.mindmap,
        abortCtrl,
      );
      return handleResponse(res, 'Mind map generation');
    } catch (err) {
      throw new Error(friendlyError(err, 'Mind map generation'));
    }
  },

  /** Generate question bank from text */
  async generateQuestionBank(text, numQuestions = 20, language = 'en', abortCtrl) {
    const fd = new FormData();
    fd.append('text', text);
    fd.append('num_questions', String(numQuestions));
    fd.append('language', language);
    try {
      const res = await fetchWithTimeout(
        `${BASE_URL}/question-bank`,
        { method: 'POST', body: fd },
        TIMEOUTS.qbank,
        abortCtrl,
      );
      return handleResponse(res, 'Question bank generation');
    } catch (err) {
      throw new Error(friendlyError(err, 'Question bank generation'));
    }
  },

  /** Generate ALL features from file in one call */
  async generateAll(file, language = 'en', numQuizQ = 10, numBankQ = 20, abortCtrl) {
    const fd = new FormData();
    fd.append('file', file);
    fd.append('language', language);
    fd.append('num_quiz_questions', String(numQuizQ));
    fd.append('num_bank_questions', String(numBankQ));
    try {
      const res = await fetchWithTimeout(
        `${BASE_URL}/generate-all`,
        { method: 'POST', body: fd },
        TIMEOUTS.generateAll,
        abortCtrl,
      );
      return handleResponse(res, 'Generate All');
    } catch (err) {
      throw new Error(friendlyError(err, 'Generate All'));
    }
  },

  /** Streaming chat with callback
   *  @param {string}          filename
   *  @param {string}          userMessage
   *  @param {string}          context
   *  @param {string}          language
   *  @param {Function}        onChunk     — called with each text chunk
   *  @param {AbortController} [abortCtrl] — pass to allow cancel mid-stream
   */
  async streamChat(filename, userMessage, context, language = 'en', onChunk, abortCtrl) {
    const arabicPattern = /[\u0600-\u06FF]/;
    const detectedLang = arabicPattern.test(userMessage) ? 'eg' : language;

    const ctrl = abortCtrl || new AbortController();
    const timer = setTimeout(() => ctrl.abort(), TIMEOUTS.chat);

    try {
      const res = await fetch(`${BASE_URL}/chat`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          filename,
          user_message: userMessage,
          context,
          language: detectedLang,
        }),
        signal: ctrl.signal,
      });

      if (!res.ok) {
        let msg = 'Chat error';
        try { const d = await res.json(); msg = d.error || msg; } catch {}
        throw new Error(msg);
      }

      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        onChunk(decoder.decode(value, { stream: true }));
      }
    } catch (err) {
      throw new Error(friendlyError(err, 'Chat'));
    } finally {
      clearTimeout(timer);
    }
  },
};