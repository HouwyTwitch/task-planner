import{a as fe,b as ke,c as we,f as ve,g as Ge}from"./chunk-SO4LWOHM.js";import{C as me,D as Ne,E as he,F as De,L as be,M as Be,a as pe,b as o,c as Le}from"./chunk-35E3OWQW.js";import{p as le,q as Ee}from"./chunk-OJE3YBZP.js";import{a as ge,b as He}from"./chunk-CLATF5NV.js";import{a as Re}from"./chunk-35DJQS47.js";import{c as ue,d as z,g as E,i as F}from"./chunk-2R4LUUTQ.js";import{e as ae,g as ie,o as Me}from"./chunk-IFMKKORO.js";import{Cb as de,Hb as Oe,Jb as ce,Lb as $e,d as te,e as re,g as Se,w as oe,x as Ae}from"./chunk-LNYDCYOQ.js";import{a as se,b as Te}from"./chunk-VSJMWCNC.js";import{d as s,h as ne}from"./chunk-M3ULHVXU.js";import{$c as Z,Cb as q,Dc as N,Ea as f,Ec as p,Ed as M,Fc as b,Fd as T,Gc as D,Hb as h,La as R,Ma as L,Nc as B,Tc as G,Vc as k,Ya as S,Yb as X,Zc as Q,_c as Y,fd as U,hb as K,id as C,ie as ee,jd as A,kd as j,uc as J,wc as y,yc as x}from"./chunk-NII6HHRB.js";import{a as P,b as $,e as H,k as v}from"./chunk-ID73HXZW.js";var Ce,_e=H(()=>{"use strict";Ce=`
<style id="sp-ui-kit">
  *, *::before, *::after {
    box-sizing: border-box;
  }

  body {
    font-size: 14px;
    line-height: 1.4;
    margin: 0;
  }

  h1 { font-size: 1.8em; font-weight: 700; margin: 0 0 var(--s2) 0; }
  h2 { font-size: 1.4em; font-weight: 700; margin: 0 0 var(--s2) 0; }
  h3 { font-size: 1.2em; font-weight: 700; margin: 0 0 var(--s) 0; }
  h4 { font-size: 1.05em; font-weight: 700; margin: 0 0 var(--s) 0; }
  h5 { font-size: 0.95em; font-weight: 400; margin: 0 0 var(--s) 0; }
  h6 { font-size: 0.85em; font-weight: 400; margin: 0 0 var(--s) 0; }

  p {
    margin: 0 0 var(--s2) 0;
    line-height: 1.5;
  }

  button {
    background: var(--card-bg);
    color: var(--text-color);
    border: 1px solid var(--divider-color);
    border-radius: var(--card-border-radius);
    padding: var(--s-half) var(--s2);
    cursor: pointer;
    font-family: inherit;
    font-size: inherit;
    transition: var(--transition-standard);
  }

  button:hover {
    background: var(--select-hover-bg);
    border-color: var(--c-primary);
  }

  button:active {
    filter: brightness(0.92);
  }

  button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  button.btn-primary {
    background: var(--c-primary);
    border-color: var(--c-primary);
    color: #fff;
  }

  button.btn-primary:hover {
    filter: brightness(1.12);
  }

  button.btn-outline {
    background: transparent;
    border-color: var(--c-primary);
    color: var(--c-primary);
  }

  button.btn-outline:hover {
    background: var(--c-primary);
    color: #fff;
  }

  input, textarea, select {
    background: var(--card-bg);
    color: var(--text-color);
    border: none;
    border-bottom: 1px solid var(--divider-color);
    border-radius: var(--card-border-radius) var(--card-border-radius) 0 0;
    padding: var(--s) var(--s2);
    font-family: inherit;
    font-size: inherit;
    transition: var(--transition-standard);
    width: 100%;
  }

  input:focus, textarea:focus, select:focus {
    outline: none;
    border-bottom-color: var(--c-primary);
    border-bottom-width: 2px;
  }

  input:disabled, textarea:disabled, select:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  input[type="checkbox"], input[type="radio"] {
    width: auto;
    accent-color: var(--c-primary);
  }

  label {
    display: block;
    margin-bottom: var(--s-half);
    font-size: 0.85em;
    color: var(--text-color-muted);
    cursor: pointer;
  }

  select {
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23888' d='M6 8L1 3h10z'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right var(--s) center;
    padding-right: var(--s4);
  }

  a {
    color: var(--c-primary);
    text-decoration: none;
  }

  a:hover {
    text-decoration: underline;
  }

  ul, ol {
    padding-left: var(--s3);
    margin: 0 0 var(--s2) 0;
  }

  table {
    border-collapse: collapse;
    width: 100%;
  }

  th, td {
    padding: var(--s) var(--s2);
    border-bottom: 1px solid var(--divider-color);
    text-align: left;
  }

  hr {
    border: none;
    border-top: 1px solid var(--divider-color);
    margin: var(--s2) 0;
  }

  code {
    background: var(--bg-darker);
    padding: var(--s-quarter) var(--s-half);
    border-radius: 2px;
    font-size: 0.9em;
  }

  pre {
    background: var(--bg-darker);
    padding: var(--s2);
    border-radius: var(--card-border-radius);
    overflow-x: auto;
  }

  pre > code {
    background: none;
    padding: 0;
  }

  .card {
    background: var(--card-bg);
    border-radius: var(--card-border-radius);
    box-shadow: var(--whiteframe-shadow-2dp);
    padding: var(--s2);
    text-align: left;
    border: 2px solid var(--extra-border-color);
  }

  .card-clickable:hover {
    transition: transform 0.2s;
    transform: translateY(-2px);
    box-shadow: var(--whiteframe-shadow-4dp);
    border-color: var(--c-primary);
    cursor: pointer;
  }

  ::selection {
    background: color-mix(in srgb, var(--c-primary) 30%, transparent);
  }

  ::placeholder {
    color: var(--text-color-muted);
  }

  /* Utility classes */
  .text-muted { color: var(--text-color-muted); }
  .text-primary { color: var(--c-primary); }

  /* Page transition */
  .page-fade { animation: fadeIn 0.3s ease; }
  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }
</style>
`});var Ue,ye,je,W,ze,Fe,xe,Ie,We,Pe=H(()=>{"use strict";Le();ne();_e();Ue=new Set(["getTasks","getArchivedTasks","getCurrentContextTasks","getSelectedTask","getFocusedTask","selectTask","reInitData","updateTask","addTask","deleteTask","batchUpdateForProject","reorderTasks","getAllProjects","addProject","updateProject","getAllTags","addTag","updateTag","showSnack","notify","request","translate","formatDate","getCurrentLanguage","openDialog","showIndexHtmlAsView","showIndexHtmlInSidePanel","showInWorkContext","closeWorkContextView","getActiveWorkContext","getAppState","persistDataSynced","loadPersistedData","registerHook","registerWorkContextHeaderButton","executeNodeScript","dispatchAction","setCounter","getCounter","incrementCounter","decrementCounter","deleteCounter","getAllCounters"]),ye=t=>{let r=t.trim().replace(/\s+/g,"").toLowerCase();return!r||r==="transparent"||/^#[\da-f]{3}0$/.test(r)||/^#[\da-f]{6}00$/.test(r)||/^.+\/0(?:\.0+)?%?\)$/.test(r)?!0:/^(?:rgba?|hsla?)\([^,]+,[^,]+,[^,]+,0(?:\.0+)?%?\)$/.test(r)},je=(t,r,e)=>ye(t)?ye(r)?e:r:t,W="__hasDialogButtonHandler",ze=()=>{let t=document.body.classList.contains("isDarkTheme"),r=getComputedStyle(document.body),e=c=>r.getPropertyValue(c).trim(),n=e("--bg"),a=e("--bg-lighter"),i=je(e("--card-bg"),a,n);return`
    <style id="injected-theme-vars">
      :root {
        --bg: ${n};
        --bg-darker: ${e("--bg-darker")};
        --bg-lighter: ${a};
        --text-color: ${e("--text-color")};
        --text-color-less-intense: ${e("--text-color-less-intense")};
        --text-color-muted: ${e("--text-color-muted")};
        --card-bg: ${i};
        --card-shadow: ${e("--card-shadow")};
        --card-border-radius: ${e("--card-border-radius")};
        --divider-color: ${e("--divider-color")};
        --extra-border-color: ${e("--extra-border-color")};
        --select-hover-bg: ${e("--select-hover-bg")};
        --c-primary: ${e("--c-primary")};
        --c-accent: ${e("--c-accent")};
        --c-warn: ${e("--c-warn")};
        --color-success: ${e("--color-success")};
        --color-danger: ${e("--color-danger")};
        --color-warning: ${e("--color-warning")};
        --is-dark-theme: ${t?"1":"0"};
        --scrollbar-thumb: ${e("--scrollbar-thumb")};
        --scrollbar-thumb-hover: ${e("--scrollbar-thumb-hover")};
        --scrollbar-track: ${e("--scrollbar-track")};

        /* Shadow system */
        --whiteframe-shadow-1dp: ${e("--whiteframe-shadow-1dp")};
        --whiteframe-shadow-2dp: ${e("--whiteframe-shadow-2dp")};
        --whiteframe-shadow-3dp: ${e("--whiteframe-shadow-3dp")};
        --whiteframe-shadow-4dp: ${e("--whiteframe-shadow-4dp")};
        --whiteframe-shadow-6dp: ${e("--whiteframe-shadow-6dp")};
        --whiteframe-shadow-8dp: ${e("--whiteframe-shadow-8dp")};
        --whiteframe-shadow-12dp: ${e("--whiteframe-shadow-12dp")};
        --whiteframe-shadow-24dp: ${e("--whiteframe-shadow-24dp")};

        /* Spacing system */
        --s: ${e("--s")};
        --s-quarter: ${e("--s-quarter")};
        --s-half: ${e("--s-half")};
        --s2: ${e("--s2")};
        --s3: ${e("--s3")};
        --s4: ${e("--s4")};

        /* Transition system */
        --transition-duration-xs: ${e("--transition-duration-xs")};
        --transition-duration-s: ${e("--transition-duration-s")};
        --transition-duration-m: ${e("--transition-duration-m")};
        --transition-duration-l: ${e("--transition-duration-l")};
        --transition-standard: ${e("--transition-standard")};
        --ani-standard-timing: ${e("--ani-standard-timing")};

        /* Font stack */
        --font-primary-stack: ${e("--font-primary-stack")};

        /* Task-related variables */
        --task-first-line-min-height: ${e("--task-first-line-min-height")};
        --task-icon-default-opacity: ${e("--task-icon-default-opacity")};
        --task-inner-padding-top-bottom: ${e("--task-inner-padding-top-bottom")};
        --task-is-done-dim-opacity: ${e("--task-is-done-dim-opacity")};
        --task-border-radius: ${e("--task-border-radius")};
        --task-c-bg: ${e("--task-c-bg")};
        --task-c-selected-bg: ${e("--task-c-selected-bg")};
        --sub-task-c-bg: ${e("--sub-task-c-bg")};
        --sub-task-c-bg-done: ${e("--sub-task-c-bg-done")};
        --task-c-bg-done: ${e("--task-c-bg-done")};
        --task-c-current-bg: ${e("--task-c-current-bg")};
        --task-c-drag-drop-bg: ${e("--task-c-drag-drop-bg")};
        --sub-task-c-bg-in-selected: ${e("--sub-task-c-bg-in-selected")};
        --task-shadow: ${e("--task-shadow")};
        --task-shadow-sub-task: ${e("--task-shadow-sub-task")};
        --task-current-shadow: ${e("--task-current-shadow")};
        --task-selected-shadow: ${e("--task-selected-shadow")};
        --task-detail-value-color: ${e("--task-detail-value-color")};
        --task-detail-bg: ${e("--task-detail-bg")};
        --task-detail-bg-hover: ${e("--task-detail-bg-hover")};
        --task-detail-shadow: ${e("--task-detail-shadow")};
      }

      body {
        background: transparent;
        color: var(--text-color);
        font-family: var(--font-primary-stack);
      }

      /* Custom scrollbar styles for plugins */
      :root {
        scrollbar-color: var(--scrollbar-thumb) var(--scrollbar-track);
        scrollbar-width: thin;
      }

      ::-webkit-scrollbar {
        width: 4px;
        height: 4px;
      }

      /* Track */
      ::-webkit-scrollbar-track {
        background: var(--scrollbar-track);
        border-radius: 4px;
      }

      /* Handle */
      ::-webkit-scrollbar-thumb {
        background: var(--scrollbar-thumb);
        border-radius: 16px;
      }

      /* Handle on hover */
      ::-webkit-scrollbar-thumb:hover {
        background: var(--scrollbar-thumb-hover);
      }
    </style>
  `},Fe=t=>`
    <script>
      (function() {
        let callId = 0;
        const bridgeToken = ${JSON.stringify(t.bridgeToken)};
        const bridgeGeneration = ${JSON.stringify(t.bridgeGeneration)};
        const pendingCalls = new Map();
        const dialogButtonHandlers = new Map();
        const hookHandlers = new Map(); // Store hook handlers by hook type
        const workContextBtnHandlers = new Map(); // button label -> onClick

        // Handle responses from parent
        window.addEventListener('message', function(event) {
          // Accept messages only from the host (our parent window).
          // Without this check, any other iframe / sibling window could
          // spoof API responses, hook events, or work-context button
          // clicks. The host always posts via the iframe's contentWindow,
          // so event.source will be window.parent in the legitimate case.
          if (event.source !== window.parent) return;
          const data = event.data;
          if (data?.type === '${o.API_RESPONSE}' && data.callId) {
            const resolver = pendingCalls.get(data.callId);
            if (resolver) {
              pendingCalls.delete(data.callId);
              resolver.resolve(data.result);
            }
          } else if (data?.type === '${o.API_ERROR}' && data.callId) {
            const resolver = pendingCalls.get(data.callId);
            if (resolver) {
              pendingCalls.delete(data.callId);
              resolver.reject(new Error(data.error));
            }
          } else if (data?.type === '${o.DIALOG_BUTTON_CLICK}') {
            // Handle dialog button clicks
            const key = data.dialogCallId + ':' + data.buttonIndex;
            const handler = dialogButtonHandlers.get(key);
            if (handler) {
              Promise.resolve()
                .then(() => handler())
                .then((result) => {
                  window.parent.postMessage({
                    type: '${o.DIALOG_BUTTON_RESPONSE}',
                    bridgeToken: bridgeToken,
                    bridgeGeneration: bridgeGeneration,
                    dialogCallId: data.dialogCallId,
                    buttonIndex: data.buttonIndex,
                    result: result
                  }, '*');
                })
                .catch((error) => {
                  window.parent.postMessage({
                    type: '${o.DIALOG_BUTTON_RESPONSE}',
                    bridgeToken: bridgeToken,
                    bridgeGeneration: bridgeGeneration,
                    dialogCallId: data.dialogCallId,
                    buttonIndex: data.buttonIndex,
                    error: error instanceof Error ? error.message : 'Unknown dialog button error'
                  }, '*');
                });
            }
          } else if (data?.type === '${o.WORK_CONTEXT_BTN_CLICK}') {
            const handler = workContextBtnHandlers.get(data.buttonHandlerId);
            if (handler) {
              try {
                handler(data.ctx);
              } catch (error) {
                console.error('Plugin work-context button handler error:', error);
              }
            }
          } else if (data?.type === '${o.HOOK_EVENT}') {
            // Handle hook events
            const handlers = hookHandlers.get(data.hook);
            if (handlers && handlers.length > 0) {
              handlers.forEach(handler => {
                try {
                  handler(data.payload);
                } catch (error) {
                  // console, not PluginLog: this runs inside the iframe, where
                  // the host's PluginLog global does not exist.
                  console.error('Plugin hook handler error:', error);
                }
              });
            }
          }
        });

        // Simple API call function
        function callApi(method, args) {
          return new Promise((resolve, reject) => {
            const id = ++callId;
            pendingCalls.set(id, { resolve, reject });

            // Special handling for openDialog to store button handlers
            let processedArgs = args;
            if (method === 'openDialog' && args && args[0] && args[0].buttons) {
              processedArgs = [...args];
              processedArgs[0] = {
                ...args[0],
                buttons: args[0].buttons.map((button, index) => {
                  if (button.onClick) {
                    // Store the handler
                    const key = id + ':' + index;
                    dialogButtonHandlers.set(key, button.onClick);
                    // Remove onClick from serialized data
                    const { onClick, ...buttonWithoutHandler } = button;
                    return { ...buttonWithoutHandler, ${W}: true };
                  }
                  return button;
                })
              };
            }

            // Special handling for registerHook to store handlers locally
            if (method === 'registerHook' && args && args.length >= 2) {
              const [hook, handler] = args;
              if (typeof handler === 'function') {
                // Store handler locally
                if (!hookHandlers.has(hook)) {
                  hookHandlers.set(hook, []);
                }
                hookHandlers.get(hook).push(handler);

                // Pass a placeholder to parent - parent will send events back to us
                processedArgs = [hook, 'IFRAME_HANDLER'];
              } else {
                processedArgs = args;
              }
            }

            window.parent.postMessage({
              type: '${o.API_CALL}',
              bridgeToken: bridgeToken,
              bridgeGeneration: bridgeGeneration,
              method: method,
              args: processedArgs || [],
              callId: id
            }, '*');

            // Timeout after 30 seconds
            setTimeout(() => {
              if (pendingCalls.has(id)) {
                pendingCalls.delete(id);
                reject(new Error('API call timeout'));
              }
            }, 30000);
          });
        }

        function unsupportedIframeRegistration(method) {
          return () => Promise.reject(
            new Error(method + ' is only supported in plugin.js, not iframe index.html')
          );
        }

        // Create the PluginAPI object with all methods
        window.PluginAPI = {
          cfg: ${JSON.stringify(t.baseCfg)},

          // Add Hooks enum (kept in sync with PluginHooks via JSON.stringify
          // of the real source \u2014 no hand-edited mirror to drift).
          Hooks: ${JSON.stringify(P({},pe))},

          // Task methods
          getTasks: () => callApi('getTasks'),
          getArchivedTasks: () => callApi('getArchivedTasks'),
          getCurrentContextTasks: () => callApi('getCurrentContextTasks'),
          getSelectedTask: () => callApi('getSelectedTask'),
          getFocusedTask: () => callApi('getFocusedTask'),
          selectTask: (taskId) => callApi('selectTask', [taskId]),
          reInitData: () => callApi('reInitData'),
          updateTask: (taskId, updates) => callApi('updateTask', [taskId, updates]),
          addTask: (taskData) => callApi('addTask', [taskData]),
          deleteTask: (taskId) => callApi('deleteTask', [taskId]),
          batchUpdateForProject: (request) => callApi('batchUpdateForProject', [request]),

          // Project methods
          getAllProjects: () => callApi('getAllProjects'),
          addProject: (projectData) => callApi('addProject', [projectData]),
          updateProject: (projectId, updates) => callApi('updateProject', [projectId, updates]),

          // Tag methods
          getAllTags: () => callApi('getAllTags'),
          addTag: (tagData) => callApi('addTag', [tagData]),
          updateTag: (tagId, updates) => callApi('updateTag', [tagId, updates]),

          // Task ordering
          reorderTasks: (taskIds, contextId, contextType) => callApi('reorderTasks', [taskIds, contextId, contextType]),

          // UI methods
          showSnack: (cfg) => callApi('showSnack', [cfg]),
          notify: (cfg) => callApi('notify', [cfg]),
          request: (url, options) => callApi('request', [url, options]),
          openDialog: (cfg) => callApi('openDialog', [cfg]),
          showIndexHtmlAsView: () => callApi('showIndexHtmlAsView'),
          showIndexHtmlInSidePanel: () => callApi('showIndexHtmlInSidePanel'),
          showInWorkContext: () => callApi('showInWorkContext'),
          closeWorkContextView: () => callApi('closeWorkContextView'),
          getActiveWorkContext: () => callApi('getActiveWorkContext'),
          getAppState: () => callApi('getAppState'),

          // Persistence methods (optional second arg = key, forwarded by callApi)
          persistDataSynced: (data, key) => callApi('persistDataSynced', [data, key]),
          loadPersistedData: (key) => callApi('loadPersistedData', [key]),
          loadSyncedData: (key) => callApi('loadPersistedData', [key]), // Alias

          // Registration methods
          registerHook: (hook, handler) => callApi('registerHook', [hook, handler]),
          registerHeaderButton: unsupportedIframeRegistration('registerHeaderButton'),
          registerMenuEntry: unsupportedIframeRegistration('registerMenuEntry'),
          registerConfigHandler: unsupportedIframeRegistration('registerConfigHandler'),
          registerShortcut: unsupportedIframeRegistration('registerShortcut'),
          registerSidePanelButton: unsupportedIframeRegistration('registerSidePanelButton'),
          registerWorkContextHeaderButton: (cfg) => {
            // onClick is not structured-cloneable across postMessage; keep it
            // locally, keyed by the button's label, and send the rest of the
            // cfg. The host rebuilds onClick as a proxy and posts a
            // WORK_CONTEXT_BTN_CLICK back (carrying the label) on invocation.
            // Keying by label \u2014 which the host also dedups on \u2014 means
            // re-registering a button overwrites instead of leaking.
            workContextBtnHandlers.set(cfg.label, cfg.onClick);
            const { onClick, ...rest } = cfg;
            return callApi('registerWorkContextHeaderButton', [rest]);
          },

          // Node execution (if available)
          executeNodeScript: (request) => callApi('executeNodeScript', [request]),

          // Action dispatch
          dispatchAction: (action) => callApi('dispatchAction', [action]),

          // Simple counters
          setCounter: (id, value) => callApi('setCounter', [id, value]),
          getCounter: (id) => callApi('getCounter', [id]),
          incrementCounter: (id, incrementBy) => callApi('incrementCounter', [id, incrementBy]),
          decrementCounter: (id, decrementBy) => callApi('decrementCounter', [id, decrementBy]),
          deleteCounter: (id) => callApi('deleteCounter', [id]),
          getAllCounters: () => callApi('getAllCounters'),

          // i18n
          translate: (key, params) => callApi('translate', [key, params]),
          formatDate: (date, format) => callApi('formatDate', [date, format]),
          getCurrentLanguage: () => callApi('getCurrentLanguage'),

          // Readiness signal for iframe plugins.
          //
          // NOTE \u2014 semantic difference from host-side onReady:
          // The host implementation pings the Electron IPC bridge with retry before
          // firing the callback (handles cold-boot races for nodeExecution plugins).
          // Here, we just fire on the next microtask. This is acceptable because:
          //   1. Iframe plugins are rendered on user navigation, long after host
          //      startup \u2014 the cold-boot window has already passed.
          //   2. executeNodeScript calls in iframe plugins proxy through the host
          //      via callApi(); the host applies its own ping logic per call site.
          // If iframe plugins ever auto-render at startup, route this through a
          // host-side RPC that calls PluginService._fireOnReady.
          onReady: (fn) => {
            queueMicrotask(() => {
              try {
                Promise.resolve(fn()).catch((err) => {
                  console.error('[Plugin] onReady callback error:', err);
                });
              } catch (err) {
                console.error('[Plugin] onReady callback error:', err);
              }
            });
          },

          // Teardown signal \u2014 no-op in iframes: the host unmounts the iframe on
          // unload, which takes its timers/listeners with it. Provided so plugin
          // code can call onUnload unconditionally on both execution paths.
          onUnload: (fn) => {},

          // Message handling
          onMessage: (handler) => {
            // Store the handler and set up message listener
            window.__pluginMessageHandler = handler;
            window.addEventListener('message', async (event) => {
              // Same origin-check rationale as the listener above \u2014
              // accept only messages from our parent host.
              if (event.source !== window.parent) return;
              if (event.data?.type === '${o.MESSAGE}' && window.__pluginMessageHandler) {
                try {
                  const result = await window.__pluginMessageHandler(event.data.message);
                  event.source?.postMessage({
                    type: '${o.MESSAGE_RESPONSE}',
                    messageId: event.data.messageId,
                    result
                  }, '*');
                } catch (error) {
                  event.source?.postMessage({
                    type: '${o.MESSAGE_ERROR}',
                    messageId: event.data.messageId,
                    error: error instanceof Error ? error.message : 'Unknown error'
                  }, '*');
                }
              }
            });
          }
        };

        // Notify parent that plugin is ready
        window.parent.postMessage({
          type: '${o.READY}',
          bridgeToken: bridgeToken,
          bridgeGeneration: bridgeGeneration,
          pluginId: '${t.pluginId}'
        }, '*');
      })();
    <\/script>
  `,xe=t=>{let r=Fe(t),a=ze()+(t.manifest.uiKit!==!1?Ce:"")+r,i=t.indexHtml,c=i.match(/<head[^>]*>/i);if(c){let u=c.index+c[0].length;return i.slice(0,u)+a+i.slice(u)}return a+i},Ie=(t,r)=>v(null,null,function*(){let{data:e}=t;if(!(!e||typeof e!="object")&&We(e,r)){if(r.boundMethods||(r.boundMethods=r.pluginBridge.createBoundMethods(r.pluginId,r.manifest)),e.type===o.API_CALL&&e.callId){let{method:n,args:a=[],callId:i}=e;try{if(!Ue.has(n))throw new Error(`Unknown API method: ${n}`);if(n==="registerWorkContextHeaderButton"&&a.length>=1){let d=a[0],g=d.label;if(typeof g!="string"||!g)throw new Error("registerWorkContextHeaderButton requires a string label");let l=$(P({},d),{onClick:O=>{t.source?.postMessage({type:o.WORK_CONTEXT_BTN_CLICK,buttonHandlerId:g,ctx:O},"*")}}),_=r.boundMethods.registerWorkContextHeaderButton,I=yield _(l);t.source?.postMessage({type:o.API_RESPONSE,callId:i,result:I},{targetOrigin:"*"});return}if(n==="registerHook"){let d=r.pluginBridge;if(a.length>=2){let[g,l]=a;s.log("Plugin iframe registerHook:",{hook:g,handlerPlaceholder:l,pluginId:r.pluginId});let m;if(l==="IFRAME_HANDLER"){let _=I=>{t.source?.postMessage({type:o.HOOK_EVENT,hook:g,payload:I},"*")};m=yield d.registerHook(r.pluginId,g,_)}else throw new Error("Iframe registerHook calls must use IFRAME_HANDLER");t.source?.postMessage({type:o.API_RESPONSE,callId:i,result:m},{targetOrigin:"*"});return}}let c=r.boundMethods;if(c&&typeof c[n]=="function"){let d=yield c[n](...a||[]);t.source?.postMessage({type:o.API_RESPONSE,callId:i,result:d},"*");return}let u=r.pluginBridge;if(typeof u[n]!="function")throw new Error(`Unknown API method: ${n}`);if(n==="openDialog"&&a.length>=1){let d=a[0];d.buttons&&(d.buttons=d.buttons.map((l,m)=>{if(l[W]){let _=P({},l);return delete _[W],$(P({},_),{onClick:()=>v(null,null,function*(){return t.source?.postMessage({type:o.DIALOG_BUTTON_CLICK,buttonIndex:m,dialogCallId:i},{targetOrigin:"*"}),new Promise((I,O)=>{let V=w=>{w.source===t.source&&w.data?.bridgeToken===r.bridgeToken&&w.data?.bridgeGeneration===r.bridgeGeneration&&w.data?.type===o.DIALOG_BUTTON_RESPONSE&&w.data?.dialogCallId===i&&w.data?.buttonIndex===m&&(window.removeEventListener("message",V),w.data.error?O(new Error(w.data.error)):I(w.data.result))};window.addEventListener("message",V)})})})}return l}));let g=yield u[n](d);t.source?.postMessage({type:o.API_RESPONSE,callId:i,result:g},"*")}else{let d=yield u[n](...a||[]);t.source?.postMessage({type:o.API_RESPONSE,callId:i,result:d},"*")}}catch(c){t.source?.postMessage({type:o.API_ERROR,callId:i,error:c instanceof Error?c.message:"Unknown error"},"*")}}if(e.type===o.MESSAGE&&e.messageId)try{let n=yield r.pluginBridge.sendMessageToPlugin(r.pluginId,e.message);t.source?.postMessage({type:o.MESSAGE_RESPONSE,messageId:e.messageId,result:n},"*")}catch(n){t.source?.postMessage({type:o.MESSAGE_ERROR,messageId:e.messageId,error:n instanceof Error?n.message:"Unknown error"},"*")}e.type===o.READY&&e.pluginId===r.pluginId&&s.log(`Plugin ${r.pluginId} is ready`)}}),We=(t,r)=>!new Set([o.API_CALL,o.MESSAGE,o.READY]).has(t.type)||t.type===o.MESSAGE?!0:t.bridgeToken===r.bridgeToken&&t.bridgeGeneration===r.bridgeGeneration});function Ke(t,r){if(t&1&&(p(0,"mat-card",3)(1,"mat-card-content")(2,"div",5),D(3,"mat-progress-spinner",6),p(4,"p"),C(5),M(6,"translate"),b()()()()),t&2){let e=k(2);h(5),A(T(6,1,e.T.PLUGINS.LOADING_INTERFACE))}}function qe(t,r){if(t&1&&(p(0,"div",4),D(1,"mat-progress-spinner",7),p(2,"p"),C(3),M(4,"translate"),b()()),t&2){let e=k(2);h(3),A(T(4,1,e.T.PLUGINS.LOADING_PLUGIN))}}function Xe(t,r){if(t&1&&y(0,Ke,7,3,"mat-card",3)(1,qe,5,3,"div",4),t&2){let e=k();x(e.showFullUI?0:1)}}function Je(t,r){if(t&1){let e=B();p(0,"mat-card",8)(1,"mat-card-header")(2,"mat-card-title")(3,"mat-icon",10),C(4,"error"),b(),C(5),M(6,"translate"),b()(),p(7,"mat-card-content")(8,"p"),C(9),b(),p(10,"button",11),G("click",function(){R(e);let a=k(2);return L(a.goBack())}),C(11),M(12,"translate"),b()()()}if(t&2){let e=k(2);h(5),j(" ",T(6,3,e.T.PLUGINS.ERROR_LOADING_PLUGIN)," "),h(4),A(e.error()),h(2),j(" ",T(12,5,e.T.PLUGINS.GO_BACK)," ")}}function Qe(t,r){if(t&1&&(p(0,"div",9)(1,"mat-icon"),C(2,"error_outline"),b(),p(3,"p"),C(4),b()()),t&2){let e=k(2);h(4),A(e.error())}}function Ye(t,r){if(t&1&&y(0,Je,13,7,"mat-card",8)(1,Qe,5,1,"div",9),t&2){let e=k();x(e.showFullUI?0:1)}}function Ze(t,r){if(t&1){let e=B();p(0,"div",12)(1,"iframe",13,0),G("load",function(){R(e);let a=k();return L(a.onIframeLoad())}),b()()}if(t&2){let e=k();U("is-resizing",e.isResizing()),h(),U("hidden",e.isLoading()),N("srcdoc",e.iframeSrcdoc(),q),J("data-plugin-id",e.pluginId())}}var Ve,At,et=H(()=>{"use strict";ee();Me();Ae();Re();Be();Ne();De();Pe();Ge();Se();Te();ne();Ee();Oe();$e();He();ee();Ve=["iframe"];At=(()=>{class t{constructor(){this.showFullUI=!0,this.useSidePanelConfig=!1,this.skipCleanupOnDestroy=!1,this._route=f(ae),this._router=f(ie),this._elRef=f(K),this._sanitizer=f(oe),this._pluginService=f(be),this._pluginBridge=f(me),this._cleanupService=f(he),this._translateService=f(te),this._layoutService=f(le),this.T=se,this.pluginId=S(""),this.isLoading=S(!0),this.error=S(null),this.iframeSrcdoc=S(null),this.isResizing=this._layoutService.isPanelResizing}ngOnInit(){return v(this,null,function*(){if(this.directPluginId){this.pluginId.set(this.directPluginId),yield this._waitForPluginSystem();try{yield this._loadPluginIndex(this.directPluginId)}catch(e){s.err("Failed to load plugin index:",e),this.error.set(e instanceof Error?e.message:"Failed to load plugin"),this.isLoading.set(!1)}return}this._routeSubscription=this._route.paramMap.subscribe(e=>v(this,null,function*(){let n=e.get("pluginId");if(s.log("Route paramMap changed, newPluginId:",n,"currentPluginId:",this.pluginId()),!n){this.error.set("Plugin ID not provided"),this.isLoading.set(!1);return}if(this.pluginId()===n){s.log("Same plugin ID, skipping reload");return}s.log(`Navigating from plugin "${this.pluginId()}" to plugin "${n}"`),this._cleanupIframeCommunication(),this.isLoading.set(!0),this.error.set(null),this.pluginId.set(n),yield this._waitForPluginSystem();try{yield this._loadPluginIndex(n)}catch(a){s.err("Failed to load plugin index:",a),this.error.set(a instanceof Error?a.message:"Failed to load plugin"),this.isLoading.set(!1)}}))})}_waitForPluginSystem(){return v(this,null,function*(){if(!this._pluginService.isInitialized())try{yield this._pluginService.initializePlugins()}catch(e){throw s.err("Failed to initialize plugin system:",e),new Error("Failed to initialize plugin system")}})}ngOnDestroy(){this._cleanupIframeCommunication(),this._routeSubscription&&this._routeSubscription.unsubscribe()}goBack(){this._router.navigate(["/config"],{fragment:"plugins"})}_loadPluginIndex(e){return v(this,null,function*(){s.log(`Loading plugin index for: ${e}`);let n=this._pluginService.getPluginIndexHtml(e);if(!n){s.err(`No index.html content found for plugin: ${e}`);let l=(yield this._pluginService.getAllPlugins()).find(m=>m.manifest.id===e);throw l?l.manifest.iFrame?l.error?new Error(l.error):new Error("Plugin index.html not loaded"):new Error("Plugin does not support iframes"):new Error("Plugin not found")}let i=(yield this._pluginService.getAllPlugins()).find(g=>g.manifest.id===e);if(!i)throw new Error("Plugin not found");let c=yield this._pluginService.getBaseCfg(),u={pluginId:e,manifest:i.manifest,indexHtml:n,baseCfg:c,pluginBridge:this._pluginBridge,bridgeToken:this._createBridgeToken(),bridgeGeneration:this._pluginService.getPluginIframeGeneration(e),boundMethods:this._pluginBridge.createBoundMethods(e,i.manifest)},d=xe(u);this._messageListener=g=>v(this,null,function*(){let l=g,m=this._getPluginIframeWindow();!m||l.source!==m||this._pluginService.getPluginIframeGeneration(u.pluginId)===u.bridgeGeneration&&this._pluginService.getPluginIndexHtml(u.pluginId)&&(yield Ie(l,u))}),window.addEventListener("message",this._messageListener),this.iframeSrcdoc.set(this._sanitizer.bypassSecurityTrustHtml(d)),this.isLoading.set(!1),s.log(`Plugin ${e} iframe srcdoc set, loading complete`)})}_createBridgeToken(){let e=new Uint8Array(16);return crypto.getRandomValues(e),Array.from(e,n=>n.toString(16).padStart(2,"0")).join("")}_getPluginIframeWindow(){return this._elRef.nativeElement.querySelector("iframe[data-plugin-iframe]")?.contentWindow??null}_cleanupIframeCommunication(){let e=this.pluginId();s.log(`Cleaning up iframe communication for plugin: ${e}`),this._messageListener&&(window.removeEventListener("message",this._messageListener),this._messageListener=void 0,s.log(`Removed message listener for plugin: ${e}`)),e&&!this.skipCleanupOnDestroy&&(this._cleanupService.cleanupPlugin(e),s.log(`Cleaned up plugin references for: ${e}`)),this.iframeSrcdoc.set(this._sanitizer.bypassSecurityTrustHtml("<html><body></body></html>")),s.log(`Reset iframe to empty document for plugin: ${e}`)}onIframeLoad(){s.log("Plugin iframe loaded for plugin:",this.pluginId()),this.iframeRef?.nativeElement&&this.pluginId()&&this._cleanupService.registerIframe(this.pluginId(),this.iframeRef.nativeElement)}static{this.\u0275fac=function(n){return new(n||t)}}static{this.\u0275cmp=X({type:t,selectors:[["plugin-index"]],viewQuery:function(n,a){if(n&1&&Q(Ve,5),n&2){let i;Y(i=Z())&&(a.iframeRef=i.first)}},inputs:{directPluginId:"directPluginId",showFullUI:"showFullUI",useSidePanelConfig:"useSidePanelConfig",skipCleanupOnDestroy:"skipCleanupOnDestroy"},decls:4,vars:4,consts:[["iframe",""],[1,"plugin-index-container"],[1,"iframe-container",3,"is-resizing"],[1,"loading-card"],[1,"simple-loading"],[1,"loading-content"],["mode","indeterminate","diameter","40"],["mode","indeterminate","diameter","24"],[1,"error-card"],[1,"simple-error"],["color","warn"],["mat-button","","color","primary",3,"click"],[1,"iframe-container"],["data-plugin-iframe","","sandbox","allow-scripts allow-same-origin allow-forms allow-popups allow-modals",1,"plugin-iframe",3,"load","srcdoc"]],template:function(n,a){n&1&&(p(0,"div",1),y(1,Xe,2,1),y(2,Ye,2,1),y(3,Ze,3,6,"div",2),b()),n&2&&(N("@pluginSwitch",a.pluginId()),h(),x(a.isLoading()?1:-1),h(),x(a.error()?2:-1),h(),x(a.iframeSrcdoc()&&!a.error()?3:-1))},dependencies:[de,ce,ge,fe,we,ve,ke,re],styles:[`@media screen and (min-width:600px){.isNoMac[_ngcontent-%COMP%]{scrollbar-color:var(--scrollbar-thumb) var(--scrollbar-track);scrollbar-width:4px}.isNoMac[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar{width:4px;height:4px}.isNoMac[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-track{background:var(--scrollbar-track);border-radius:var(--card-border-radius)}.isNoMac[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-thumb{background:var(--scrollbar-thumb);border-radius:16px}.isNoMac[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover{background:var(--scrollbar-thumb-hover)}}@media screen and (max-width:599px){.isNoTouchOnly[_ngcontent-%COMP%]{scrollbar-color:var(--scrollbar-thumb) var(--scrollbar-track);scrollbar-width:4px}.isNoTouchOnly[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar{width:4px;height:4px}.isNoTouchOnly[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-track{background:var(--scrollbar-track);border-radius:var(--card-border-radius)}.isNoTouchOnly[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-thumb{background:var(--scrollbar-thumb);border-radius:16px}.isNoTouchOnly[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover{background:var(--scrollbar-thumb-hover)}}.plugin-index-container[_ngcontent-%COMP%]{height:100%;display:flex;flex-direction:column;overflow:hidden}.loading-card[_ngcontent-%COMP%], .error-card[_ngcontent-%COMP%]{margin:2rem;max-width:600px;align-self:center}.loading-content[_ngcontent-%COMP%]{display:flex;flex-direction:column;align-items:center;gap:1rem;padding:2rem}.loading-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%]{margin:0;color:var(--text-color-muted)}.error-card[_ngcontent-%COMP%]   .mat-card-header[_ngcontent-%COMP%]   .mat-card-title[_ngcontent-%COMP%]{display:flex;align-items:center;gap:.5rem;color:var(--color-danger)}.error-card[_ngcontent-%COMP%]   p[_ngcontent-%COMP%]{margin-bottom:1rem;color:var(--text-color)}.simple-loading[_ngcontent-%COMP%], .simple-error[_ngcontent-%COMP%]{display:flex;flex-direction:column;align-items:center;justify-content:center;height:100%;gap:var(--s2)}.simple-error[_ngcontent-%COMP%]{color:var(--color-danger)}.simple-error[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%]{font-size:48px;width:48px;height:48px}.simple-loading[_ngcontent-%COMP%]   p[_ngcontent-%COMP%], .simple-error[_ngcontent-%COMP%]   p[_ngcontent-%COMP%]{margin:0;opacity:.7}.iframe-container[_ngcontent-%COMP%]{flex:1;position:relative;overflow:hidden}.iframe-container.is-resizing[_ngcontent-%COMP%], .iframe-container.is-resizing[_ngcontent-%COMP%]   iframe[_ngcontent-%COMP%]{pointer-events:none}.plugin-iframe[_ngcontent-%COMP%]{width:100%;height:100%;border:none;background:transparent;scrollbar-color:var(--scrollbar-thumb) var(--scrollbar-track);scrollbar-width:4px}.plugin-iframe[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar{width:4px;height:4px}.plugin-iframe[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-track{background:var(--scrollbar-track);border-radius:var(--card-border-radius)}.plugin-iframe[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-thumb{background:var(--scrollbar-thumb);border-radius:16px}.plugin-iframe[_ngcontent-%COMP%]   [_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover{background:var(--scrollbar-thumb-hover)}.plugin-iframe.hidden[_ngcontent-%COMP%]{visibility:hidden}
/*# sourceMappingURL=plugin-index.component-UZMBGD2U.css.map */`],data:{animation:[ue("pluginSwitch",[F(":enter",[E({opacity:0,transform:"translateX(20px)"}),z("300ms ease-in-out",E({opacity:1,transform:"translateX(0)"}))]),F("* => *",[E({opacity:0,transform:"translateX(-20px)"}),z("300ms ease-in-out",E({opacity:1,transform:"translateX(0)"}))])])]},changeDetection:0})}}return t})()});export{At as a,et as b};
//# sourceMappingURL=chunk-IBCDEPMG.js.map
