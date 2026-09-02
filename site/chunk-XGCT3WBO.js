import{b as Ue,c as Ge,g as Ti}from"./chunk-5HKCF3B5.js";import{L as Ke,M as Pi,p as qe,q as Fi}from"./chunk-35E3OWQW.js";import{g as Ve,h as Di}from"./chunk-A2CC5FQU.js";import{a as G,b as pt,c as je,d as Ft,p as Mi}from"./chunk-F6FBDVQ6.js";import{A as ki,T as Ie,U as xi,Ua as ht,Va as ze,rb as Ne,sb as Li,z as Oe}from"./chunk-P576EGSD.js";import{a as Fe,b as Ii,c as Pe,d as Ai}from"./chunk-MTANKBYO.js";import{a as P,d as Se}from"./chunk-MEB5R5AC.js";import{V as st,W as we,X as Ce,Y as vi,a as Dt,b as si}from"./chunk-IFMKKORO.js";import{F as Re,O as Ei,y as Be}from"./chunk-VTZ47GKD.js";import{$a as ui,E as fe,I as be,Ia as di,Jb as De,Ka as ve,Lb as Oi,Ma as hi,Ta as F,Va as pi,Yb as dt,Zb as Te,_a as ke,d as he,g as oi,hb as yi,mb as Ae,ob as wi,qb as Ee,r as pe,rb as Le,tb as Si,u as ci,ub as Me,vb as Ci,w as ue,x as li,y as ge}from"./chunk-LNYDCYOQ.js";import{a as _e,d as mi}from"./chunk-WOFO3NZJ.js";import{j as ye,k as _i,l as gi}from"./chunk-3QJ37AIW.js";import{a as w,b as xe}from"./chunk-VSJMWCNC.js";import{a as Et,b as ai,c as ct,e as fi}from"./chunk-RDQOKAYZ.js";import{a as I,q as Lt}from"./chunk-NBKIJOHT.js";import{a as mt,b as bi}from"./chunk-5SIXCHI2.js";import{c as de,d as ri}from"./chunk-FTZBFH3Y.js";import{a as k,g as Mt,h as ot}from"./chunk-M3ULHVXU.js";import{$c as T,Ca as kt,Cb as ae,D as Qt,Dc as Ct,E as Wt,Ea as p,Ec as U,F as Y,Fc as et,Gc as it,Hb as yt,K as Xt,Od as me,P as Yt,Pa as ee,Ra as ie,Tc as re,Ua as ne,Ud as rt,Vc as nt,W as Jt,Wc as Ot,X as Zt,Xc as M,Ya as z,Yb as j,Yc as It,Zc as oe,_b as A,_c as D,a as Kt,bb as xt,bc as V,ed as At,fb as N,fd as at,hb as Z,id as se,ie as O,jd as ce,l as R,ma as J,n as Ht,pa as te,s as W,t as $t,ta as vt,u as X,uc as tt,wc as wt,xd as le,yc as St,za as L}from"./chunk-NII6HHRB.js";import{a as lt,b as Tt}from"./chunk-HRTVIJB6.js";import{a as _,b as y,c as qt,d as Q,e as C,k as b}from"./chunk-ID73HXZW.js";function Bi(i){if(!isFinite(i))return 0;let s=1,t=0;for(;Math.round(i*s)/s!==i;)s*=10,t++;return t}function S(i){return i===""||i==null}function q(i){return i!=null&&typeof i=="object"&&!Array.isArray(i)}function Ri(i){return Number.isInteger?Number.isInteger(i):typeof i=="number"&&Math.floor(i)===i}function Pt(i){return typeof i=="object"&&(i.hasOwnProperty("const")||i.enum&&i.enum.length===1)}function zi(i){if(i===""||i===void 0)return;if(i===null)return null;if(typeof i=="number")return i;let s=parseFloat(i);return isNaN(s)?i:s}function ut(i){if(!i.fieldGroup)return G(i)&&pt(i)!==void 0?1:0;let s=i.fieldGroup.reduce((t,e)=>ut(e)+t,0);if(s===0&&G(i)){let t=pt(i);if(t===null||t!==void 0&&(i.fieldArray&&Array.isArray(t)||!i.fieldArray&&q(t)))return 1}return s}var En,Ni=C(()=>{"use strict";O();Ei();Mi();xt();En=(()=>{class i{toFieldConfig(t,e){return t=Ft(t),this._toFieldConfig(t,_({schema:t},e||{}))}_toFieldConfig(t,c){var d=c,{key:e,isOptional:n}=d,a=Q(d,["key","isOptional"]);t=this.resolveSchema(t,a);let m=this.guessSchemaType(t),r={type:m[0],defaultValue:t.default,props:{label:t.title,readonly:t.readOnly,description:t.description}};if(e!=null&&(r.key=e),!a.ignoreDefault&&(t.readOnly||a.readOnly)&&(r.props.disabled=!0,a=y(_({},a),{readOnly:!0})),a.resetOnHide&&(r.resetOnHide=!0),a.shareFormControl===!1&&(r.shareFormControl=!1),r.defaultValue===void 0&&m.length===1&&n===!1)switch(m[0]){case"null":{r.defaultValue=null;break}case"string":{r.defaultValue="";break}case"object":{r.defaultValue={};break}case"array":{r.defaultValue=t.minItems>0?Array.from(new Array(t.minItems)):[];break}}switch(a.ignoreDefault&&delete r.defaultValue,this.addValidator(r,"type",{schemaType:m,expression:({value:o})=>{if(o===void 0||o===null&&m.indexOf("null")!==-1)return!0;switch(m[0]){case"null":return typeof o===null;case"string":return typeof o=="string";case"integer":return Ri(o);case"number":return typeof o=="number";case"object":return q(o);case"array":return Array.isArray(o)}return!0}}),r.type){case"number":case"integer":{r.parsers=[(o,h)=>{if(o=zi(o),o===null&&h){let l=typeof document<"u"&&h.id?document.querySelector(`#${h.id}`):void 0;l&&l.validity&&!l.validity.badInput&&(o=void 0),o!==h.formControl.value&&h.formControl.setValue(o,{emitModelToViewChange:!1})}return o}],t.hasOwnProperty("minimum")&&(r.props.min=t.minimum),t.hasOwnProperty("maximum")&&(r.props.max=t.maximum),t.hasOwnProperty("exclusiveMinimum")&&(r.props.exclusiveMinimum=t.exclusiveMinimum,this.addValidator(r,"exclusiveMinimum",({value:o})=>S(o)||o>t.exclusiveMinimum)),t.hasOwnProperty("exclusiveMaximum")&&(r.props.exclusiveMaximum=t.exclusiveMaximum,this.addValidator(r,"exclusiveMaximum",({value:o})=>S(o)||o<t.exclusiveMaximum)),t.hasOwnProperty("multipleOf")&&(r.props.step=t.multipleOf,this.addValidator(r,"multipleOf",({value:o})=>{if(S(o)||typeof o!="number"||o===0||t.multipleOf<=0)return!0;let h=Math.pow(10,Bi(t.multipleOf));return Math.round(o*h)%Math.round(t.multipleOf*h)===0}));break}case"string":{r.parsers=[(o,h)=>(m.indexOf("null")!==-1?o=S(o)?null:o:h&&!h.props.required&&(o=o===""?void 0:o),o)],["minLength","maxLength","pattern"].forEach(o=>{t.hasOwnProperty(o)&&(r.props[o]=t[o])});break}case"object":{r.fieldGroup||(r.fieldGroup=[]);let{propDeps:o,schemaDeps:h}=this.resolveDependencies(t);Object.keys(t.properties||{}).forEach(l=>{let g=Array.isArray(t.required)&&t.required.indexOf(l)!==-1,f=this._toFieldConfig(t.properties[l],y(_({},a),{key:l,isOptional:n||!g}));if(r.fieldGroup.push(f),(g||o[l])&&(f.expressions=y(_({},f.expressions||{}),{"props.required":v=>{let u=v.parent,x=v.fieldGroup&&v.key!=null?u.model:v.model;for(;u.key==null&&u.parent;)u=u.parent;let E=u&&u.props?u.props.required:!1;return!x&&!E?!1:Array.isArray(t.required)&&t.required.indexOf(l)!==-1?!0:o[l]&&v.model&&o[l].some(B=>!S(v.model[B]))}})),h[l]){let v=x=>x.hasOwnProperty("const")?x.const:x.enum[0],u=h[l].oneOf;u&&u.every(x=>x.properties&&x.properties[l]&&Pt(x.properties[l]))?u.forEach(x=>{let Ut=x.properties,{[l]:E}=Ut,B=Q(Ut,[qt(l)]);r.fieldGroup.push(y(_({},this._toFieldConfig(y(_({},x),{properties:B}),y(_({},a),{shareFormControl:!1,resetOnHide:!0}))),{expressions:{hide:Gt=>!Gt.model||v(E)!==Gt.model[l]}}))}):r.fieldGroup.push(y(_({},this._toFieldConfig(h[l],a)),{expressions:{hide:x=>!x.model||S(x.model[l])}}))}}),t.oneOf&&r.fieldGroup.push(this.resolveMultiSchema("oneOf",t.oneOf,y(_({},a),{shareFormControl:!1}))),t.anyOf&&r.fieldGroup.push(this.resolveMultiSchema("anyOf",t.anyOf,a)),a.conditionalSchemas&&a.conditionalSchemas.forEach(g=>{let f=g._ifCondition;if(f&&g.properties){let v={fieldGroup:[],expressions:{hide:u=>{if(!u.model)return!0;let E=u.model[f.property]===f.value;return f.negate?E:!E}}};Object.keys(g.properties).forEach(u=>{let x=g.properties[u];if(!x)return;let E=Array.isArray(g.required)&&g.required.indexOf(u)!==-1,B=this._toFieldConfig(x,y(_({},a),{key:u,isOptional:!E,resetOnHide:!0}));v.fieldGroup.push(B)}),r.fieldGroup.push(v)}});break}case"array":{t.hasOwnProperty("minItems")&&(r.props.minItems=t.minItems,this.addValidator(r,"minItems",({value:o})=>S(o)||o.length>=t.minItems),!n&&t.minItems>0&&r.defaultValue===void 0&&(r.defaultValue=Array.from(new Array(t.minItems)))),t.hasOwnProperty("maxItems")&&(r.props.maxItems=t.maxItems,this.addValidator(r,"maxItems",({value:o})=>S(o)||o.length<=t.maxItems)),t.hasOwnProperty("uniqueItems")&&(r.props.uniqueItems=t.uniqueItems,this.addValidator(r,"uniqueItems",({value:o})=>S(o)||!t.uniqueItems?!0:Array.from(new Set(o.map(l=>JSON.stringify(l,(g,f)=>q(f)?Object.keys(f).sort().reduce((v,u)=>(v[u]=f[u],v),{}):f)))).length===o.length)),t.items&&!Array.isArray(t.items)&&(t.items=this.resolveSchema(t.items,a)),this.isEnum(t)||(r.fieldArray=o=>{let h=o.fieldGroup?o.fieldGroup.length:0,l=t.items;if(!Array.isArray(l)){if(!l)return{};let v=l.oneOf||l.anyOf,u=this._toFieldConfig(l,v?y(_({},a),{key:`${h}`,isOptional:!1}):y(_({},a),{isOptional:!1}));return v&&!G(u)&&(u.key=null),u}let g=l[h]?l[h]:t.additionalItems,f=g?this._toFieldConfig(g,a):{};return f.props&&(f.props.required=!0),l[h]&&(f.props.removable=!1),f});break}}if(t.hasOwnProperty("const")&&(r.props.const=t.const,this.addValidator(r,"const",({value:o})=>o===t.const),r.type||(r.defaultValue=t.const)),this.isEnum(t)){let o=this.toEnumOptions(t),h=r.type==="array";r.type="enum",r.props.multiple=h,r.props.options=o;let l=o.map(g=>g.value);this.addValidator(r,"enum",({value:g})=>g===void 0?!0:h?Array.isArray(g)?g.every(f=>l.includes(f)):!1:l.includes(g))}return t.oneOf&&!r.type&&(delete r.key,r.fieldGroup=[this.resolveMultiSchema("oneOf",t.oneOf,y(_({},a),{key:e,shareFormControl:!1}))]),t.anyOf&&!r.type&&(delete r.key,r.fieldGroup=[this.resolveMultiSchema("oneOf",t.anyOf,y(_({},a),{key:e,shareFormControl:!1}))]),t.widget?.formlyConfig&&(r=this.mergeFields(r,t.widget.formlyConfig)),r.templateOptions=r.props,a.map?a.map(r,t):r}resolveSchema(t,e){if(t&&t.$ref&&(t=this.resolveDefinition(t,e)),t&&t.allOf&&(t=this.resolveAllOf(t,e)),t&&(t.if||t.then||t.else)){let n=this.resolveIfThenElse(t,e);n.length>0&&(e.conditionalSchemas=n)}return t}resolveAllOf(a,n){var c=a,{allOf:t}=c,e=Q(c,["allOf"]);if(!t.length)throw Error(`allOf array can not be empty ${t}.`);return t.reduce((d,m)=>(m=this.resolveSchema(m,n),d.required&&m.required&&(d.required=[...d.required,...m.required]),m.uniqueItems&&(d.uniqueItems=m.uniqueItems),["maxLength","maximum","exclusiveMaximum","maxItems","maxProperties"].forEach(r=>{!S(d[r])&&!S(m[r])&&(d[r]=d[r]<m[r]?d[r]:m[r])}),["minLength","minimum","exclusiveMinimum","minItems","minProperties"].forEach(r=>{!S(d[r])&&!S(m[r])&&(d[r]=d[r]>m[r]?d[r]:m[r])}),je(d,m)),e)}resolveMultiSchema(t,e,n){return{type:"multischema",fieldGroup:[{type:"enum",defaultValue:-1,props:{multiple:t==="anyOf",options:e.map((a,c)=>({label:a.title,value:c,disabled:a.readOnly}))},hooks:{onInit:a=>a.formControl.valueChanges.pipe(te(()=>a.options.detectChanges(a.parent)))}},{fieldGroup:e.map((a,c)=>y(_({},this._toFieldConfig(a,y(_({},n),{resetOnHide:!0}))),{expressions:{hide:(d,m)=>{let r=d.parent.parent.fieldGroup[0].formControl;if(r.value===-1||m){let o=d.parent.fieldGroup.map((h,l)=>[h,l,this.isFieldValid(h,l,e,n)]).sort(([h,,l],[g,,f])=>{if(l!==f)return f?1:-1;let v=ut(h),u=ut(g);return v===u&&h.props.disabled===g.props.disabled?0:u>v?1:-1}).map(([,h])=>h);if(t==="anyOf"){let h=o.filter(l=>ut(d.parent.fieldGroup[l]));o=h.length>0?h:[o[0]||0]}o=o.length>0?o:[0],r.setValue(t==="anyOf"?o:o[0])}return Array.isArray(r.value)?r.value.indexOf(c)===-1:r.value!==c}}}))}]}}resolveDefinition(t,e){let[n,a]=t.$ref.split("#/");if(n)throw Error(`Remote schemas for ${t.$ref} not supported yet.`);let c=a?a.split("/").reduce((d,m)=>d?.hasOwnProperty(m)?d[m]:null,e.schema):null;if(!c)throw Error(`Cannot find a definition for ${t.$ref}.`);return c.$ref?this.resolveDefinition(c,e):_(_({},c),["title","description","default","widget"].reduce((d,m)=>(t.hasOwnProperty(m)&&(d[m]=t[m]),d),{}))}resolveDependencies(t){let e={},n={};return Object.keys(t.dependencies||{}).forEach(a=>{let c=t.dependencies[a];Array.isArray(c)?c.forEach(d=>{e[d]?e[d].push(a):e[d]=[a]}):n[a]=c}),{propDeps:e,schemaDeps:n}}extractIfCondition(t){if(t.properties){let e=Object.keys(t.properties)[0];if(e){let n=t.properties[e];if(n&&n.hasOwnProperty("const"))return{property:e,value:n.const}}}return null}resolveIfThenElse(t,e){let n=[];if(t.if&&typeof t.if=="object"){let a=this.extractIfCondition(t.if);if(a){if(t.then&&typeof t.then=="object"){let c=this.resolveConditionalSchema(t.then,e);n.push(y(_({},c),{_ifCondition:a}))}if(t.else&&typeof t.else=="object"){let c=this.resolveConditionalSchema(t.else,e);n.push(y(_({},c),{_ifCondition:{property:a.property,value:a.value,negate:!0}}))}}}return n}resolveConditionalSchema(t,e){let n=t;return t.$ref&&(n=this.resolveDefinition(t,e)),n.allOf&&(n=this.resolveAllOf(n,e)),n}guessSchemaType(t){let e=t?.type;return!e&&t?.properties?["object"]:Array.isArray(e)?e.length===1?e:e.length===2&&e.indexOf("null")!==-1?e.sort(n=>n=="null"?1:-1):e:e?[e]:[]}addValidator(t,e,n){t.validators=t.validators||{},t.validators[e]=n}isEnum(t){return!!t.enum||t.anyOf&&t.anyOf.every(Pt)||t.oneOf&&t.oneOf.every(Pt)||t.uniqueItems&&t.items&&!Array.isArray(t.items)&&this.isEnum(t.items)}toEnumOptions(t){if(t.enum)return t.enum.map(n=>({value:n,label:n}));let e=n=>{let a=n.hasOwnProperty("const")?n.const:n.enum[0],c={value:a,label:n.title||a};return n.readOnly&&(c.disabled=!0),c};return t.anyOf?t.anyOf.map(e):t.oneOf?t.oneOf.map(e):this.toEnumOptions(t.items)}isFieldValid(t,e,n,a){let c=n[e];c._field||Object.defineProperty(c,"_field",{enumerable:!1,writable:!0,configurable:!0});let d=c._field,m=t.model?t.model:t.fieldArray?[]:{};return t.model&&G(t)&&(m={[Array.isArray(t.key)?t.key.join("."):t.key]:pt(t)}),m=Ft(m),d?(d.model=m,t.options.build(d)):d=c._field=t.options.build({form:Array.isArray(m)?new Re([]):new Be({}),fieldGroup:[this._toFieldConfig(c,y(_({},a),{resetOnHide:!0,ignoreDefault:!0,map:null}))],model:m,options:{}}),d.form.valid}mergeFields(t,e){for(let n in e){let a=n==="templateOptions"?"props":n;q(t[a])&&q(e[n])?t[a]=this.mergeFields(t[a],e[n]):e[n]!=null&&(t[a]=e[n])}return t}static{this.\u0275fac=function(e){return new(e||i)}}static{this.\u0275prov=L({token:i,factory:i.\u0275fac,providedIn:"root"})}}return i})()});var He,_t,Bn,ji=C(()=>{"use strict";O();Lt();ot();O();He="sp_task_widget_settings",_t={isEnabled:!1,isAlwaysShow:!1,opacity:95},Bn=(()=>{class i{constructor(){this._settings=z(this._loadFromStorage()),this.settings=this._settings.asReadonly(),I&&this._notifyElectron(this._settings())}update(t){let e=_(_({},this._settings()),t);this._settings.set(e),this._persistToStorage(e),I&&this._notifyElectron(e)}_loadFromStorage(){if(typeof localStorage>"u")return _({},_t);try{let t=localStorage.getItem(He);if(!t)return _({},_t);let e=JSON.parse(t);return _(_({},_t),e)}catch(t){return k.err("Failed to read task widget settings from localStorage",t),_({},_t)}}_persistToStorage(t){if(!(typeof localStorage>"u"))try{localStorage.setItem(He,JSON.stringify(t))}catch(e){k.err("Failed to persist task widget settings to localStorage",e)}}_notifyElectron(t){typeof window>"u"||!window.ea||window.ea.updateTaskWidgetSettings(t)}static{this.\u0275fac=function(e){return new(e||i)}}static{this.\u0275prov=L({token:i,factory:i.\u0275fac,providedIn:"root"})}}return i})()});var Vi,$e,Qe=C(()=>{"use strict";Lt();Vi=["win-store","mac-store","linux-snap"],$e=()=>{if(!I)return!1;let i=window.ea?.getDistChannel?.();return!i||!Vi.includes(i)}});var We,Xe,Ye=C(()=>{"use strict";We=i=>{let s=i.trim().replace(/^v/i,"").match(/^(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.-]+))?/);return s?{major:Number(s[1]),minor:Number(s[2]),patch:Number(s[3]),prerelease:s[4]??null}:null},Xe=(i,s)=>{let t=We(i),e=We(s);return!t||!e?!1:t.major!==e.major?t.major>e.major:t.minor!==e.minor?t.minor>e.minor:t.patch!==e.patch?t.patch>e.patch:t.prerelease===null&&e.prerelease!==null}});var Ui,Gi,qi,Ki,Hi,na,$i=C(()=>{"use strict";O();ci();vt();xt();ai();xe();we();Ye();Ai();Ii();Te();Se();ot();Qe();O();Ui="https://api.github.com/repos/super-productivity/super-productivity/releases/latest",Gi=30*1e3,qi=1440*60*1e3,Ki=15*1e3,Hi=/^v?\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/,na=(()=>{class i{constructor(){this._http=p(pe),this._globalConfigService=p(st),this._bannerService=p(Pe),this._snackService=p(dt),this._isCheckInFlight=!1}init(){$e()&&this._globalConfigService.misc$.pipe(X(t=>t?.isCheckForUpdates!==!1),Jt(),J(t=>t?Qt(Gi,qi):R)).subscribe(()=>this.checkForUpdate())}checkForUpdate(){return b(this,arguments,function*({isUserTriggered:t=!1}={}){if(!this._isCheckInFlight){this._isCheckInFlight=!0;try{let n=(yield W(this._http.get(Ui,{headers:{Accept:"application/vnd.github+json"}}).pipe($t(Ki)))).tag_name;if(!n||!Hi.test(n))throw new Error("Malformed release data");if(!Xe(n,Et.version)){t&&this._snackService.open({type:"SUCCESS",msg:w.APP.UPDATE_CHECK.UP_TO_DATE,translateParams:{version:Et.version}});return}if(!t&&localStorage.getItem(P.UPDATE_CHECK_DISMISSED_VERSION)===n)return;this._showUpdateBanner(n)}catch(e){k.log("Update check failed",{error:e?.message}),t&&this._snackService.open({type:"ERROR",msg:w.APP.UPDATE_CHECK.ERROR})}finally{this._isCheckInFlight=!1}}})}_showUpdateBanner(t){let e=`https://github.com/super-productivity/super-productivity/releases/tag/${t}`;this._bannerService.open({id:Fe.UpdateAvailable,msg:w.APP.B_UPDATE_AVAILABLE.MSG,translateParams:{version:t},ico:"file_download",isHideDismissBtn:!0,action:{label:w.APP.B_UPDATE_AVAILABLE.DOWNLOAD,fn:()=>{this._rememberVersion(t),window.ea.openExternalUrl(e)}},action2:{label:w.G.DISMISS,fn:()=>this._rememberVersion(t)}})}_rememberVersion(t){localStorage.setItem(P.UPDATE_CHECK_DISMISSED_VERSION,t)}static{this.\u0275fac=function(e){return new(e||i)}}static{this.\u0275prov=L({token:i,factory:i.\u0275fac,providedIn:"root"})}}return i})()});var Ze,Je,$,Bt,gt,H,ti,Rt,ei=C(()=>{"use strict";ze();_i();Ze=i=>{let s=i?.ids;return Array.isArray(s)?s.length:0},Je=i=>Ze(i?.task),$=i=>{if(!i||typeof i!="object")return 0;let s=i;return Ze(s.task)+Je(s.archiveYoung)+Je(s.archiveOld)},Bt=i=>{if(!i)return null;try{return $(JSON.parse(i))}catch(s){return null}},gt=i=>{if(!i)return null;try{let s=JSON.parse(i),t=s.project?.ids;return{taskCount:$(s),projectCount:Array.isArray(t)?t.filter(e=>e!==ye.id).length:0}}catch(s){return null}},H=i=>{if(!i)return!1;try{return ht(JSON.parse(i))}catch(s){return!1}},ti=i=>{if(!i)return!1;try{let t=JSON.parse(i).globalConfig?.sync;return t?t.isEnabled===!0||t.syncProvider!==null&&t.syncProvider!==void 0:!1}catch(s){return!1}},Rt=(i,s)=>H(i)?i:H(s)?s:i||s||null});var Wi,Xi,ft,zt,bt,Nt,Yi,Ji,Ta,Zi=C(()=>{"use strict";O();mi();we();vt();xt();wi();ri();Lt();si();ki();xi();Li();xe();oi();ze();ei();gi();Te();ot();fi();Di();bi();Se();O();Wi=300*1e3,Xi=30*1e3,ft="backup",zt="backup_prev",bt="super-productivity-backup.json",Nt="super-productivity-backup.prev.json",Yi=3,Ji=10,Ta=(()=>{class i{constructor(){this._destroyRef=p(ie),this._configService=p(st),this._stateSnapshotService=p(Oe),this._backupService=p(Ie),this._localDraftService=p(Ne),this._snackService=p(dt),this._translateService=p(he),this._platformService=p(Ve),this._localActions$=p(Ae),this._isAndroidWebView=p(de),this._cfg$=this._configService.cfg$.pipe(X(t=>t.localBackup)),this._triggerBackupSave$=this._cfg$.pipe(J(t=>t.isEnabled?Y(Wt(Wi),this._localActions$.pipe(Yt(Xi))):R),Zt(()=>Ht(this._backup()).pipe(Xt(t=>(k.err("LocalBackupService: Backup failed",t),R)))))}init(){this._triggerBackupSave$.pipe(_e(this._destroyRef)).subscribe()}checkBackupAvailable(){return this._isAndroidWebView?b(this,null,function*(){return(yield this._loadAndroidDbValueSafe(ft))?!0:!!(yield this._loadAndroidDbValueSafe(zt))}):this._platformService.isIOS()?this._checkBackupAvailableIOS():I?window.ea.checkBackupAvailable():Promise.resolve(!1)}loadBackupElectron(t){return window.ea.loadBackupData(t)}loadBackupAndroid(){return b(this,null,function*(){let[t,e]=yield Promise.all([this._loadAndroidDbValueSafe(ft),this._loadAndroidDbValueSafe(zt)]);return Rt(t,e)??""})}_loadAndroidDbValueSafe(t){return b(this,null,function*(){try{let e=yield this._nativeDbLoad(t);return k.log(`LocalBackupService: read Android backup '${t}' (${e?e.length:0} chars)`),e}catch(e){return k.err(`LocalBackupService: failed to read Android backup '${t}'`,e),null}})}_nativeDbLoad(t){return Dt.loadFromDbWrapped(t)}_nativeDbSave(t,e){return Dt.saveToDbWrapped(t,e)}loadBackupIOS(){return b(this,null,function*(){let[t,e]=yield Promise.all([this._readIOSFileOrNull(bt),this._readIOSFileOrNull(Nt)]);return Rt(t,e)??""})}_loadBestMobileBackupStr(){return this._isAndroidWebView?this.loadBackupAndroid():this.loadBackupIOS()}_checkBackupAvailableIOS(){return b(this,null,function*(){let[t,e]=yield Promise.all([this._iosFileExists(bt),this._iosFileExists(Nt)]);return t||e})}askForFileStoreBackupIfAvailable(){return b(this,null,function*(){if(!I&&!this._isAndroidWebView&&!this._platformService.isIOS())return;if(I){let e=yield this.checkBackupAvailable();if(typeof e!="boolean"&&ct(this._translateService.instant(w.CONFIRM.RESTORE_FILE_BACKUP,{dir:e.folder,from:new Date(e.created).toLocaleString()}))){let n=yield this.loadBackupElectron(e.path);k.log("backupData loaded from Electron backup"),yield this._importBackup(n)}return}let t=yield this._loadBestMobileBackupStr();if(t){if(H(t)&&!ti(t)){if(k.log("mobile backupData auto-restored, length: "+t.length),yield this._importBackup(t)){let n=gt(t);this._snackService.open({type:"SUCCESS",msg:w.GCF.AUTO_BACKUPS.S_AUTO_RESTORED,translateParams:{tasks:n?.taskCount??0,projects:n?.projectCount??0}})}return}ct(this._restoreMobilePromptMsg(t))&&(k.log("mobile backupData loaded, length: "+t.length),yield this._importBackup(t))}})}restoreLatestMobileBackupFromSettings(){return b(this,null,function*(){if(!this._isAndroidWebView&&!this._platformService.isIOS())return;let t=yield this._loadBestMobileBackupStr();if(!H(t)){this._snackService.open({type:"WARNING",msg:w.GCF.AUTO_BACKUPS.S_NO_BACKUP_AVAILABLE});return}ct(this._restoreMobileFromSettingsPromptMsg(t))&&(k.log("mobile backupData loaded from settings, length: "+t.length),(yield this._importBackup(t))&&this._snackService.open({type:"SUCCESS",msg:w.GCF.AUTO_BACKUPS.S_RESTORE_SUCCESS}))})}_restoreMobilePromptMsg(t){let e=gt(t);return e?this._translateService.instant(w.CONFIRM.RESTORE_FILE_BACKUP_MOBILE,{tasks:e.taskCount,projects:e.projectCount}):this._translateService.instant(w.CONFIRM.RESTORE_FILE_BACKUP_ANDROID)}_restoreMobileFromSettingsPromptMsg(t){let e=gt(t);return this._translateService.instant(w.CONFIRM.RESTORE_FILE_BACKUP_MOBILE_FROM_SETTINGS,{tasks:e?.taskCount??0,projects:e?.projectCount??0})}_backup(){return b(this,null,function*(){let t=yield this._stateSnapshotService.getAllSyncModelDataFromStoreAsync();if(!ht(t)){k.warn("LocalBackupService: skipping backup \u2014 empty state");return}let e=!1;I&&(yield this._backupElectron(t),e=!0),this._isAndroidWebView&&(yield this._backupAndroid(t))&&(e=!0),this._platformService.isIOS()&&(yield this._backupIOS(t))&&(e=!0),e&&this._recordLastBackupTime()})}_recordLastBackupTime(){try{localStorage.setItem(P.LAST_LOCAL_BACKUP,Date.now().toString())}catch(t){k.warn("LocalBackupService: failed to record last backup time",t)}}getLastBackupTime(){let t=localStorage.getItem(P.LAST_LOCAL_BACKUP);if(!t)return null;let e=Number(t);return Number.isFinite(e)?e:null}_backupElectron(t){return b(this,null,function*(){let e=yield W(this._cfg$);yield window.ea.backupAppData({data:t,maxBackupFiles:e.maxBackupFiles??51})})}_isNearEmptyOverwrite(t,e){if($(t)>=Yi)return!1;let n=Bt(e);return n===null?!1:n>=Ji}_guardNearEmptyOverwrite(t,e,n){return this._isNearEmptyOverwrite(t,e)?(k.warn(`LocalBackupService: skipping ${n} backup \u2014 near-empty (${$(t)} tasks) over substantial backup (${Bt(e)} tasks). #7925 A3.`),!0):!1}_backupAndroid(t){return b(this,null,function*(){let e;try{e=yield this._nativeDbLoad(ft)}catch(a){return k.err("LocalBackupService: skipping Android backup \u2014 could not read existing slot",a),!1}if(this._guardNearEmptyOverwrite(t,e,"Android"))return!1;e&&(yield this._nativeDbSave(zt,e));let n=JSON.stringify(t);return k.log(`LocalBackupService: writing Android backup (${n.length} chars)`),yield this._nativeDbSave(ft,n),!0})}_backupIOS(t){return b(this,null,function*(){let e;try{e=yield this._readIOSExistingSlotOrThrow(bt)}catch(n){return k.err("LocalBackupService: skipping iOS backup \u2014 could not read existing slot",n),!1}if(this._guardNearEmptyOverwrite(t,e,"iOS"))return!1;try{return e&&(yield this._writeIOSFile(Nt,e)),yield this._writeIOSFile(bt,JSON.stringify(t)),k.log("iOS backup saved successfully"),!0}catch(n){return k.err("Failed to save iOS backup",n),!1}})}_writeIOSFile(t,e){return b(this,null,function*(){yield mt.writeFile({path:t,data:e,directory:lt.Data,encoding:Tt.UTF8})})}_readIOSFileRaw(t){return b(this,null,function*(){return(yield mt.readFile({path:t,directory:lt.Data,encoding:Tt.UTF8})).data})}_readIOSFileOrNull(t){return b(this,null,function*(){try{return yield this._readIOSFileRaw(t)}catch(e){return null}})}_readIOSExistingSlotOrThrow(t){return b(this,null,function*(){try{return yield this._readIOSFileRaw(t)}catch(e){if(!(yield this._iosFileExists(t)))return null;throw e}})}_iosFileExists(t){return b(this,null,function*(){try{return!!(yield mt.stat({path:t,directory:lt.Data}))}catch(e){return!1}})}_importBackup(t){return b(this,null,function*(){try{return yield this._backupService.importCompleteBackup(JSON.parse(t),!1,!0,!0),this._localDraftService.deleteDraftsForActiveProfile(),!0}catch(e){return this._snackService.open({type:"ERROR",msg:w.FILE_IMEX.S_ERR_IMPORT_FAILED}),!1}})}static{this.\u0275fac=function(e){return new(e||i)}}static{this.\u0275prov=L({token:i,factory:i.\u0275fac,providedIn:"root"})}}return i})()});var tn,en,nn,an,rn,on,sn,cn,ln,mn,ii,dn,hn,pn,jt,un,_r,gr,_n=C(()=>{"use strict";O();O();pi();yi();di();vt();Si();ui();Ci();hi();tn=["*"],en=`.mdc-list {
  margin: 0;
  padding: 8px 0;
  list-style-type: none;
}
.mdc-list:focus {
  outline: none;
}

.mdc-list-item {
  display: flex;
  position: relative;
  justify-content: flex-start;
  overflow: hidden;
  padding: 0;
  align-items: stretch;
  cursor: pointer;
  padding-left: 16px;
  padding-right: 16px;
  background-color: var(--mat-list-list-item-container-color, transparent);
  border-radius: var(--mat-list-list-item-container-shape, var(--mat-sys-corner-none));
}
.mdc-list-item.mdc-list-item--selected {
  background-color: var(--mat-list-list-item-selected-container-color);
}
.mdc-list-item:focus {
  outline: 0;
}
.mdc-list-item.mdc-list-item--disabled {
  cursor: auto;
}
.mdc-list-item.mdc-list-item--with-one-line {
  height: var(--mat-list-list-item-one-line-container-height, 48px);
}
.mdc-list-item.mdc-list-item--with-one-line .mdc-list-item__start {
  align-self: center;
  margin-top: 0;
}
.mdc-list-item.mdc-list-item--with-one-line .mdc-list-item__end {
  align-self: center;
  margin-top: 0;
}
.mdc-list-item.mdc-list-item--with-two-lines {
  height: var(--mat-list-list-item-two-line-container-height, 64px);
}
.mdc-list-item.mdc-list-item--with-two-lines .mdc-list-item__start {
  align-self: flex-start;
  margin-top: 16px;
}
.mdc-list-item.mdc-list-item--with-two-lines .mdc-list-item__end {
  align-self: center;
  margin-top: 0;
}
.mdc-list-item.mdc-list-item--with-three-lines {
  height: var(--mat-list-list-item-three-line-container-height, 88px);
}
.mdc-list-item.mdc-list-item--with-three-lines .mdc-list-item__start {
  align-self: flex-start;
  margin-top: 16px;
}
.mdc-list-item.mdc-list-item--with-three-lines .mdc-list-item__end {
  align-self: flex-start;
  margin-top: 16px;
}
.mdc-list-item.mdc-list-item--selected::before, .mdc-list-item.mdc-list-item--selected:focus::before, .mdc-list-item:not(.mdc-list-item--selected):focus::before {
  position: absolute;
  box-sizing: border-box;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  content: "";
  pointer-events: none;
}

a.mdc-list-item {
  color: inherit;
  text-decoration: none;
}

.mdc-list-item__start {
  fill: currentColor;
  flex-shrink: 0;
  pointer-events: none;
}
.mdc-list-item--with-leading-icon .mdc-list-item__start {
  color: var(--mat-list-list-item-leading-icon-color, var(--mat-sys-on-surface-variant));
  width: var(--mat-list-list-item-leading-icon-size, 24px);
  height: var(--mat-list-list-item-leading-icon-size, 24px);
  margin-left: 16px;
  margin-right: 32px;
}
[dir=rtl] .mdc-list-item--with-leading-icon .mdc-list-item__start {
  margin-left: 32px;
  margin-right: 16px;
}
.mdc-list-item--with-leading-icon:hover .mdc-list-item__start {
  color: var(--mat-list-list-item-hover-leading-icon-color);
}
.mdc-list-item--with-leading-avatar .mdc-list-item__start {
  width: var(--mat-list-list-item-leading-avatar-size, 40px);
  height: var(--mat-list-list-item-leading-avatar-size, 40px);
  margin-left: 16px;
  margin-right: 16px;
  border-radius: 50%;
}
.mdc-list-item--with-leading-avatar .mdc-list-item__start, [dir=rtl] .mdc-list-item--with-leading-avatar .mdc-list-item__start {
  margin-left: 16px;
  margin-right: 16px;
  border-radius: 50%;
}

.mdc-list-item__end {
  flex-shrink: 0;
  pointer-events: none;
}
.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  font-family: var(--mat-list-list-item-trailing-supporting-text-font, var(--mat-sys-label-small-font));
  line-height: var(--mat-list-list-item-trailing-supporting-text-line-height, var(--mat-sys-label-small-line-height));
  font-size: var(--mat-list-list-item-trailing-supporting-text-size, var(--mat-sys-label-small-size));
  font-weight: var(--mat-list-list-item-trailing-supporting-text-weight, var(--mat-sys-label-small-weight));
  letter-spacing: var(--mat-list-list-item-trailing-supporting-text-tracking, var(--mat-sys-label-small-tracking));
}
.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  color: var(--mat-list-list-item-trailing-icon-color, var(--mat-sys-on-surface-variant));
  width: var(--mat-list-list-item-trailing-icon-size, 24px);
  height: var(--mat-list-list-item-trailing-icon-size, 24px);
}
.mdc-list-item--with-trailing-icon:hover .mdc-list-item__end {
  color: var(--mat-list-list-item-hover-trailing-icon-color);
}
.mdc-list-item.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  color: var(--mat-list-list-item-trailing-supporting-text-color, var(--mat-sys-on-surface-variant));
}
.mdc-list-item--selected.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  color: var(--mat-list-list-item-selected-trailing-icon-color, var(--mat-sys-primary));
}

.mdc-list-item__content {
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
  align-self: center;
  flex: 1;
  pointer-events: none;
}
.mdc-list-item--with-two-lines .mdc-list-item__content, .mdc-list-item--with-three-lines .mdc-list-item__content {
  align-self: stretch;
}

.mdc-list-item__primary-text {
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
  color: var(--mat-list-list-item-label-text-color, var(--mat-sys-on-surface));
  font-family: var(--mat-list-list-item-label-text-font, var(--mat-sys-body-large-font));
  line-height: var(--mat-list-list-item-label-text-line-height, var(--mat-sys-body-large-line-height));
  font-size: var(--mat-list-list-item-label-text-size, var(--mat-sys-body-large-size));
  font-weight: var(--mat-list-list-item-label-text-weight, var(--mat-sys-body-large-weight));
  letter-spacing: var(--mat-list-list-item-label-text-tracking, var(--mat-sys-body-large-tracking));
}
.mdc-list-item:hover .mdc-list-item__primary-text {
  color: var(--mat-list-list-item-hover-label-text-color, var(--mat-sys-on-surface));
}
.mdc-list-item:focus .mdc-list-item__primary-text {
  color: var(--mat-list-list-item-focus-label-text-color, var(--mat-sys-on-surface));
}
.mdc-list-item--with-two-lines .mdc-list-item__primary-text, .mdc-list-item--with-three-lines .mdc-list-item__primary-text {
  display: block;
  margin-top: 0;
  line-height: normal;
  margin-bottom: -20px;
}
.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before, .mdc-list-item--with-three-lines .mdc-list-item__primary-text::before {
  display: inline-block;
  width: 0;
  height: 28px;
  content: "";
  vertical-align: 0;
}
.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after, .mdc-list-item--with-three-lines .mdc-list-item__primary-text::after {
  display: inline-block;
  width: 0;
  height: 20px;
  content: "";
  vertical-align: -20px;
}

.mdc-list-item__secondary-text {
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
  display: block;
  margin-top: 0;
  color: var(--mat-list-list-item-supporting-text-color, var(--mat-sys-on-surface-variant));
  font-family: var(--mat-list-list-item-supporting-text-font, var(--mat-sys-body-medium-font));
  line-height: var(--mat-list-list-item-supporting-text-line-height, var(--mat-sys-body-medium-line-height));
  font-size: var(--mat-list-list-item-supporting-text-size, var(--mat-sys-body-medium-size));
  font-weight: var(--mat-list-list-item-supporting-text-weight, var(--mat-sys-body-medium-weight));
  letter-spacing: var(--mat-list-list-item-supporting-text-tracking, var(--mat-sys-body-medium-tracking));
}
.mdc-list-item__secondary-text::before {
  display: inline-block;
  width: 0;
  height: 20px;
  content: "";
  vertical-align: 0;
}
.mdc-list-item--with-three-lines .mdc-list-item__secondary-text {
  white-space: normal;
  line-height: 20px;
}
.mdc-list-item--with-overline .mdc-list-item__secondary-text {
  white-space: nowrap;
  line-height: auto;
}

.mdc-list-item--with-leading-radio.mdc-list-item,
.mdc-list-item--with-leading-checkbox.mdc-list-item,
.mdc-list-item--with-leading-icon.mdc-list-item,
.mdc-list-item--with-leading-avatar.mdc-list-item {
  padding-left: 0;
  padding-right: 16px;
}
[dir=rtl] .mdc-list-item--with-leading-radio.mdc-list-item,
[dir=rtl] .mdc-list-item--with-leading-checkbox.mdc-list-item,
[dir=rtl] .mdc-list-item--with-leading-icon.mdc-list-item,
[dir=rtl] .mdc-list-item--with-leading-avatar.mdc-list-item {
  padding-left: 16px;
  padding-right: 0;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__primary-text,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__primary-text,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines .mdc-list-item__primary-text,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines .mdc-list-item__primary-text {
  display: block;
  margin-top: 0;
  line-height: normal;
  margin-bottom: -20px;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before {
  display: inline-block;
  width: 0;
  height: 32px;
  content: "";
  vertical-align: 0;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after {
  display: inline-block;
  width: 0;
  height: 20px;
  content: "";
  vertical-align: -20px;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  display: block;
  margin-top: 0;
  line-height: normal;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before {
  display: inline-block;
  width: 0;
  height: 32px;
  content: "";
  vertical-align: 0;
}

.mdc-list-item--with-trailing-icon.mdc-list-item, [dir=rtl] .mdc-list-item--with-trailing-icon.mdc-list-item {
  padding-left: 0;
  padding-right: 0;
}
.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  margin-left: 16px;
  margin-right: 16px;
}

.mdc-list-item--with-trailing-meta.mdc-list-item {
  padding-left: 16px;
  padding-right: 0;
}
[dir=rtl] .mdc-list-item--with-trailing-meta.mdc-list-item {
  padding-left: 0;
  padding-right: 16px;
}
.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  -webkit-user-select: none;
  user-select: none;
  margin-left: 28px;
  margin-right: 16px;
}
[dir=rtl] .mdc-list-item--with-trailing-meta .mdc-list-item__end {
  margin-left: 16px;
  margin-right: 28px;
}
.mdc-list-item--with-trailing-meta.mdc-list-item--with-three-lines .mdc-list-item__end, .mdc-list-item--with-trailing-meta.mdc-list-item--with-two-lines .mdc-list-item__end {
  display: block;
  line-height: normal;
  align-self: flex-start;
  margin-top: 0;
}
.mdc-list-item--with-trailing-meta.mdc-list-item--with-three-lines .mdc-list-item__end::before, .mdc-list-item--with-trailing-meta.mdc-list-item--with-two-lines .mdc-list-item__end::before {
  display: inline-block;
  width: 0;
  height: 28px;
  content: "";
  vertical-align: 0;
}

.mdc-list-item--with-leading-radio .mdc-list-item__start,
.mdc-list-item--with-leading-checkbox .mdc-list-item__start {
  margin-left: 8px;
  margin-right: 24px;
}
[dir=rtl] .mdc-list-item--with-leading-radio .mdc-list-item__start,
[dir=rtl] .mdc-list-item--with-leading-checkbox .mdc-list-item__start {
  margin-left: 24px;
  margin-right: 8px;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__start,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__start {
  align-self: flex-start;
  margin-top: 8px;
}

.mdc-list-item--with-trailing-radio.mdc-list-item,
.mdc-list-item--with-trailing-checkbox.mdc-list-item {
  padding-left: 16px;
  padding-right: 0;
}
[dir=rtl] .mdc-list-item--with-trailing-radio.mdc-list-item,
[dir=rtl] .mdc-list-item--with-trailing-checkbox.mdc-list-item {
  padding-left: 0;
  padding-right: 16px;
}
.mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-icon, .mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-avatar,
.mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-icon,
.mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-avatar {
  padding-left: 0;
}
[dir=rtl] .mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-icon, [dir=rtl] .mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-avatar,
[dir=rtl] .mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-icon,
[dir=rtl] .mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-avatar {
  padding-right: 0;
}
.mdc-list-item--with-trailing-radio .mdc-list-item__end,
.mdc-list-item--with-trailing-checkbox .mdc-list-item__end {
  margin-left: 24px;
  margin-right: 8px;
}
[dir=rtl] .mdc-list-item--with-trailing-radio .mdc-list-item__end,
[dir=rtl] .mdc-list-item--with-trailing-checkbox .mdc-list-item__end {
  margin-left: 8px;
  margin-right: 24px;
}
.mdc-list-item--with-trailing-radio.mdc-list-item--with-three-lines .mdc-list-item__end,
.mdc-list-item--with-trailing-checkbox.mdc-list-item--with-three-lines .mdc-list-item__end {
  align-self: flex-start;
  margin-top: 8px;
}

.mdc-list-group__subheader {
  margin: 0.75rem 16px;
}

.mdc-list-item--disabled .mdc-list-item__start,
.mdc-list-item--disabled .mdc-list-item__content,
.mdc-list-item--disabled .mdc-list-item__end {
  opacity: 1;
}
.mdc-list-item--disabled .mdc-list-item__primary-text,
.mdc-list-item--disabled .mdc-list-item__secondary-text {
  opacity: var(--mat-list-list-item-disabled-label-text-opacity, 0.3);
}
.mdc-list-item--disabled.mdc-list-item--with-leading-icon .mdc-list-item__start {
  color: var(--mat-list-list-item-disabled-leading-icon-color, var(--mat-sys-on-surface));
  opacity: var(--mat-list-list-item-disabled-leading-icon-opacity, 0.38);
}
.mdc-list-item--disabled.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  color: var(--mat-list-list-item-disabled-trailing-icon-color, var(--mat-sys-on-surface));
  opacity: var(--mat-list-list-item-disabled-trailing-icon-opacity, 0.38);
}

.mat-mdc-list-item.mat-mdc-list-item-both-leading-and-trailing, [dir=rtl] .mat-mdc-list-item.mat-mdc-list-item-both-leading-and-trailing {
  padding-left: 0;
  padding-right: 0;
}

.mdc-list-item.mdc-list-item--disabled .mdc-list-item__primary-text {
  color: var(--mat-list-list-item-disabled-label-text-color, var(--mat-sys-on-surface));
}

.mdc-list-item:hover::before {
  background-color: var(--mat-list-list-item-hover-state-layer-color, var(--mat-sys-on-surface));
  opacity: var(--mat-list-list-item-hover-state-layer-opacity, var(--mat-sys-hover-state-layer-opacity));
}

.mdc-list-item.mdc-list-item--disabled::before {
  background-color: var(--mat-list-list-item-disabled-state-layer-color, var(--mat-sys-on-surface));
  opacity: var(--mat-list-list-item-disabled-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}

.mdc-list-item:focus::before {
  background-color: var(--mat-list-list-item-focus-state-layer-color, var(--mat-sys-on-surface));
  opacity: var(--mat-list-list-item-focus-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}

.mdc-list-item--disabled .mdc-radio,
.mdc-list-item--disabled .mdc-checkbox {
  opacity: var(--mat-list-list-item-disabled-label-text-opacity, 0.3);
}

.mdc-list-item--with-leading-avatar .mat-mdc-list-item-avatar {
  border-radius: var(--mat-list-list-item-leading-avatar-shape, var(--mat-sys-corner-full));
  background-color: var(--mat-list-list-item-leading-avatar-color, var(--mat-sys-primary-container));
}

.mat-mdc-list-item-icon {
  font-size: var(--mat-list-list-item-leading-icon-size, 24px);
}

@media (forced-colors: active) {
  a.mdc-list-item--activated::after {
    content: "";
    position: absolute;
    top: 50%;
    right: 16px;
    transform: translateY(-50%);
    width: 10px;
    height: 0;
    border-bottom: solid 10px;
    border-radius: 10px;
  }
  a.mdc-list-item--activated [dir=rtl]::after {
    right: auto;
    left: 16px;
  }
}

.mat-mdc-list-base {
  display: block;
}
.mat-mdc-list-base .mdc-list-item__start,
.mat-mdc-list-base .mdc-list-item__end,
.mat-mdc-list-base .mdc-list-item__content {
  pointer-events: auto;
}

.mat-mdc-list-item,
.mat-mdc-list-option {
  width: 100%;
  box-sizing: border-box;
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-list-item:not(.mat-mdc-list-item-interactive),
.mat-mdc-list-option:not(.mat-mdc-list-item-interactive) {
  cursor: default;
}
.mat-mdc-list-item .mat-divider-inset,
.mat-mdc-list-option .mat-divider-inset {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
}
.mat-mdc-list-item .mat-mdc-list-item-avatar ~ .mat-divider-inset,
.mat-mdc-list-option .mat-mdc-list-item-avatar ~ .mat-divider-inset {
  margin-left: 72px;
}
[dir=rtl] .mat-mdc-list-item .mat-mdc-list-item-avatar ~ .mat-divider-inset,
[dir=rtl] .mat-mdc-list-option .mat-mdc-list-item-avatar ~ .mat-divider-inset {
  margin-right: 72px;
}

.mat-mdc-list-item-interactive::before {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  content: "";
  opacity: 0;
  pointer-events: none;
  border-radius: inherit;
}

.mat-mdc-list-item > .mat-focus-indicator {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
.mat-mdc-list-item:focus-visible > .mat-focus-indicator::before {
  content: "";
}

.mat-mdc-list-item.mdc-list-item--with-three-lines .mat-mdc-list-item-line.mdc-list-item__secondary-text {
  white-space: nowrap;
  line-height: normal;
}
.mat-mdc-list-item.mdc-list-item--with-three-lines .mat-mdc-list-item-unscoped-content.mdc-list-item__secondary-text {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

mat-action-list button {
  background: none;
  color: inherit;
  border: none;
  font: inherit;
  outline: inherit;
  -webkit-tap-highlight-color: transparent;
  text-align: start;
}
mat-action-list button::-moz-focus-inner {
  border: 0;
}

.mdc-list-item--with-leading-icon .mdc-list-item__start {
  margin-inline-start: var(--mat-list-list-item-leading-icon-start-space, 16px);
  margin-inline-end: var(--mat-list-list-item-leading-icon-end-space, 16px);
}

.mat-mdc-nav-list .mat-mdc-list-item {
  border-radius: var(--mat-list-active-indicator-shape, var(--mat-sys-corner-full));
  --mat-focus-indicator-border-radius: var(--mat-list-active-indicator-shape, var(--mat-sys-corner-full));
}
.mat-mdc-nav-list .mat-mdc-list-item.mdc-list-item--activated {
  background-color: var(--mat-list-active-indicator-color, var(--mat-sys-secondary-container));
}
`,nn=["unscopedContent"],an=["text"],rn=[[["","matListItemAvatar",""],["","matListItemIcon",""]],[["","matListItemTitle",""]],[["","matListItemLine",""]],"*",[["","matListItemMeta",""]],[["mat-divider"]]],on=["[matListItemAvatar],[matListItemIcon]","[matListItemTitle]","[matListItemLine]","*","[matListItemMeta]","mat-divider"],sn=new kt("ListOption"),cn=(()=>{class i{_elementRef=p(Z);constructor(){}static \u0275fac=function(e){return new(e||i)};static \u0275dir=A({type:i,selectors:[["","matListItemTitle",""]],hostAttrs:[1,"mat-mdc-list-item-title","mdc-list-item__primary-text"]})}return i})(),ln=(()=>{class i{_elementRef=p(Z);constructor(){}static \u0275fac=function(e){return new(e||i)};static \u0275dir=A({type:i,selectors:[["","matListItemLine",""]],hostAttrs:[1,"mat-mdc-list-item-line","mdc-list-item__secondary-text"]})}return i})(),mn=(()=>{class i{static \u0275fac=function(e){return new(e||i)};static \u0275dir=A({type:i,selectors:[["","matListItemMeta",""]],hostAttrs:[1,"mat-mdc-list-item-meta","mdc-list-item__end"]})}return i})(),ii=(()=>{class i{_listOption=p(sn,{optional:!0});constructor(){}_isAlignedAtStart(){return!this._listOption||this._listOption?._getTogglePosition()==="after"}static \u0275fac=function(e){return new(e||i)};static \u0275dir=A({type:i,hostVars:4,hostBindings:function(e,n){e&2&&at("mdc-list-item__start",n._isAlignedAtStart())("mdc-list-item__end",!n._isAlignedAtStart())}})}return i})(),dn=(()=>{class i extends ii{static \u0275fac=(()=>{let t;return function(n){return(t||(t=N(i)))(n||i)}})();static \u0275dir=A({type:i,selectors:[["","matListItemAvatar",""]],hostAttrs:[1,"mat-mdc-list-item-avatar"],features:[V]})}return i})(),hn=(()=>{class i extends ii{static \u0275fac=(()=>{let t;return function(n){return(t||(t=N(i)))(n||i)}})();static \u0275dir=A({type:i,selectors:[["","matListItemIcon",""]],hostAttrs:[1,"mat-mdc-list-item-icon"],features:[V]})}return i})(),pn=new kt("MAT_LIST_CONFIG"),jt=(()=>{class i{_isNonInteractive=!0;get disableRipple(){return this._disableRipple}set disableRipple(t){this._disableRipple=F(t)}_disableRipple=!1;get disabled(){return this._disabled()}set disabled(t){this._disabled.set(F(t))}_disabled=z(!1);_defaultOptions=p(pn,{optional:!0});static \u0275fac=function(e){return new(e||i)};static \u0275dir=A({type:i,hostVars:1,hostBindings:function(e,n){e&2&&tt("aria-disabled",n.disabled)},inputs:{disableRipple:"disableRipple",disabled:"disabled"}})}return i})(),un=(()=>{class i{_elementRef=p(Z);_ngZone=p(ne);_listBase=p(jt,{optional:!0});_platform=p(ge);_hostElement;_isButtonElement;_noopAnimations=ke();_avatars;_icons;set lines(t){this._explicitLines=be(t,null),this._updateItemLines(!1)}_explicitLines=null;get disableRipple(){return this.disabled||this._disableRipple||this._noopAnimations||!!this._listBase?.disableRipple}set disableRipple(t){this._disableRipple=F(t)}_disableRipple=!1;get disabled(){return this._disabled()||!!this._listBase?.disabled}set disabled(t){this._disabled.set(F(t))}_disabled=z(!1);_subscriptions=new Kt;_rippleRenderer=null;_hasUnscopedTextContent=!1;rippleConfig;get rippleDisabled(){return this.disableRipple||!!this.rippleConfig.disabled}constructor(){p(fe).load(Me);let t=p(Le,{optional:!0});this.rippleConfig=t||{},this._hostElement=this._elementRef.nativeElement,this._isButtonElement=this._hostElement.nodeName.toLowerCase()==="button",this._listBase&&!this._listBase._isNonInteractive&&this._initInteractiveListItem(),this._isButtonElement&&!this._hostElement.hasAttribute("type")&&this._hostElement.setAttribute("type","button")}ngAfterViewInit(){this._monitorProjectedLinesAndTitle(),this._updateItemLines(!0)}ngOnDestroy(){this._subscriptions.unsubscribe(),this._rippleRenderer!==null&&this._rippleRenderer._removeTriggerEvents()}_hasIconOrAvatar(){return!!(this._avatars.length||this._icons.length)}_initInteractiveListItem(){this._hostElement.classList.add("mat-mdc-list-item-interactive"),this._rippleRenderer=new Ee(this,this._ngZone,this._hostElement,this._platform,p(ee)),this._rippleRenderer.setupTriggerEvents(this._hostElement)}_monitorProjectedLinesAndTitle(){this._ngZone.runOutsideAngular(()=>{this._subscriptions.add(Y(this._lines.changes,this._titles.changes).subscribe(()=>this._updateItemLines(!1)))})}_updateItemLines(t){if(!this._lines||!this._titles||!this._unscopedContent)return;t&&this._checkDomForUnscopedTextContent();let e=this._explicitLines??this._inferLinesFromContent(),n=this._unscopedContent.nativeElement;if(this._hostElement.classList.toggle("mat-mdc-list-item-single-line",e<=1),this._hostElement.classList.toggle("mdc-list-item--with-one-line",e<=1),this._hostElement.classList.toggle("mdc-list-item--with-two-lines",e===2),this._hostElement.classList.toggle("mdc-list-item--with-three-lines",e===3),this._hasUnscopedTextContent){let a=this._titles.length===0&&e===1;n.classList.toggle("mdc-list-item__primary-text",a),n.classList.toggle("mdc-list-item__secondary-text",!a)}else n.classList.remove("mdc-list-item__primary-text"),n.classList.remove("mdc-list-item__secondary-text")}_inferLinesFromContent(){let t=this._titles.length+this._lines.length;return this._hasUnscopedTextContent&&(t+=1),t}_checkDomForUnscopedTextContent(){this._hasUnscopedTextContent=Array.from(this._unscopedContent.nativeElement.childNodes).filter(t=>t.nodeType!==t.COMMENT_NODE).some(t=>!!(t.textContent&&t.textContent.trim()))}static \u0275fac=function(e){return new(e||i)};static \u0275dir=A({type:i,contentQueries:function(e,n,a){if(e&1&&It(a,dn,4)(a,hn,4),e&2){let c;D(c=T())&&(n._avatars=c),D(c=T())&&(n._icons=c)}},hostVars:4,hostBindings:function(e,n){e&2&&(tt("aria-disabled",n.disabled)("disabled",n._isButtonElement&&n.disabled||null),at("mdc-list-item--disabled",n.disabled))},inputs:{lines:"lines",disableRipple:"disableRipple",disabled:"disabled"}})}return i})(),_r=(()=>{class i extends jt{static \u0275fac=(()=>{let t;return function(n){return(t||(t=N(i)))(n||i)}})();static \u0275cmp=j({type:i,selectors:[["mat-list"]],hostAttrs:[1,"mat-mdc-list","mat-mdc-list-base","mdc-list"],exportAs:["matList"],features:[le([{provide:jt,useExisting:i}]),V],ngContentSelectors:tn,decls:1,vars:0,template:function(e,n){e&1&&(Ot(),M(0))},styles:[en],encapsulation:2,changeDetection:0})}return i})(),gr=(()=>{class i extends un{_lines;_titles;_meta;_unscopedContent;_itemText;get activated(){return this._activated}set activated(t){this._activated=F(t)}_activated=!1;_getAriaCurrent(){return this._hostElement.nodeName==="A"&&this._activated?"page":null}_hasBothLeadingAndTrailing(){return this._meta.length!==0&&(this._avatars.length!==0||this._icons.length!==0)}static \u0275fac=(()=>{let t;return function(n){return(t||(t=N(i)))(n||i)}})();static \u0275cmp=j({type:i,selectors:[["mat-list-item"],["a","mat-list-item",""],["button","mat-list-item",""]],contentQueries:function(e,n,a){if(e&1&&It(a,ln,5)(a,cn,5)(a,mn,5),e&2){let c;D(c=T())&&(n._lines=c),D(c=T())&&(n._titles=c),D(c=T())&&(n._meta=c)}},viewQuery:function(e,n){if(e&1&&oe(nn,5)(an,5),e&2){let a;D(a=T())&&(n._unscopedContent=a.first),D(a=T())&&(n._itemText=a.first)}},hostAttrs:[1,"mat-mdc-list-item","mdc-list-item"],hostVars:13,hostBindings:function(e,n){e&2&&(tt("aria-current",n._getAriaCurrent()),at("mdc-list-item--activated",n.activated)("mdc-list-item--with-leading-avatar",n._avatars.length!==0)("mdc-list-item--with-leading-icon",n._icons.length!==0)("mdc-list-item--with-trailing-meta",n._meta.length!==0)("mat-mdc-list-item-both-leading-and-trailing",n._hasBothLeadingAndTrailing())("_mat-animation-noopable",n._noopAnimations))},inputs:{activated:"activated"},exportAs:["matListItem"],features:[V],ngContentSelectors:on,decls:10,vars:0,consts:[["unscopedContent",""],[1,"mdc-list-item__content"],[1,"mat-mdc-list-item-unscoped-content",3,"cdkObserveContent"],[1,"mat-focus-indicator"]],template:function(e,n){e&1&&(Ot(rn),M(0),U(1,"span",1),M(2,1),M(3,2),U(4,"span",2,0),re("cdkObserveContent",function(){return n._updateItemLines(!0)}),M(6,3),et()(),M(7,4),M(8,5),it(9,"div",3))},dependencies:[ve],encapsulation:2,changeDetection:0})}return i})()});var ni,gn,fn,Vt,vr,bn=C(()=>{"use strict";vi();ni=0,gn=["username","userName","loginName","password","token","apiKey","secret","authorization","notes","authCode","accessToken","host","gitlabBaseUrl","nextcloudBaseUrl","icalUrl","organization","syncFilePath","syncFolderPath","title","originalImgPath","path","content","repo","repoFullname","filterUserName","filterUsername","caldavUrl","api_key","resourceName","name","description","location","calProviderId","summary","filterIncludeRegex","filterExcludeRegex"],fn=(i,s,t)=>gn.includes(i)&&s.length>0?`${i}__${t}`:s,Vt=i=>{if(!(typeof i!="object"||i===null))for(let s in i){if(Object.prototype.hasOwnProperty.call(i,s)){let t=i[s];Array.isArray(t)?t.forEach(e=>{typeof e=="object"&&e!==null&&Vt(e)}):typeof t=="object"&&t!==null?Vt(t):typeof t=="string"&&(i[s]=fn(s,t,ni))}ni++}},vr=i=>{let s=Ce(i);return Vt(s),JSON.stringify(s)}});var vn,kn,xn,wr,yn=C(()=>{"use strict";ot();Ti();vn="./assets/snd",kn=50,xn=300,wr=(i,s=0)=>b(null,null,function*(){let t=`${vn}/${i.doneSound}`;Mt.log(t);let e=i.isIncreaseDoneSoundPitch?Math.min(s*kn,xn):0;try{let n=yield Ue(t);yield Ge(n,i.volume,a=>{a.detune.value=e})}catch(n){Mt.err("Error playing done sound:",n)}})});function wn(i,s){if(i&1&&it(0,"div",2),i&2){let t=nt();At("width",t.size()+"px")("height",t.size()+"px"),Ct("innerHTML",s,ae)}}function Sn(i,s){if(i&1&&(U(0,"mat-icon"),se(1),et()),i&2){let t=nt();At("font-size",t.size()+"px"),yt(),ce(t.fallbackIcon())}}var Tr,Cn=C(()=>{"use strict";O();li();Pi();Oi();Fi();O();Tr=(()=>{class i{constructor(){this._sanitizer=p(ue),this._pluginService=p(Ke),this.pluginId=rt.required(),this.size=rt(24),this.fallbackIcon=rt("extension"),this.sanitizedSvg=me(()=>{let t=this.pluginId(),n=this._pluginService.getPluginIconsSignal()().get(t);if(!n)return null;let a=qe(n);return a?this._sanitizer.bypassSecurityTrustHtml(a):null})}static{this.\u0275fac=function(e){return new(e||i)}}static{this.\u0275cmp=j({type:i,selectors:[["plugin-icon"]],inputs:{pluginId:[1,"pluginId"],size:[1,"size"],fallbackIcon:[1,"fallbackIcon"]},decls:2,vars:1,consts:[[1,"plugin-svg-icon",3,"innerHTML","width","height"],[3,"font-size"],[1,"plugin-svg-icon",3,"innerHTML"]],template:function(e,n){if(e&1&&wt(0,wn,1,5,"div",0)(1,Sn,2,3,"mat-icon",1),e&2){let a;St((a=n.sanitizedSvg())?0:1,a)}},dependencies:[De],styles:[`[_nghost-%COMP%]{display:inline-flex;align-items:center;justify-content:center}[_nghost-%COMP%]   mat-icon[_ngcontent-%COMP%]{display:flex;flex:1}.plugin-svg-icon[_ngcontent-%COMP%]{display:flex;align-items:center;justify-content:center}.plugin-svg-icon[_ngcontent-%COMP%]     svg{width:100%;height:100%;fill:currentColor}
/*# sourceMappingURL=plugin-icon.component-YDIBNI2S.css.map */`],changeDetection:0})}}return i})()});export{vr as a,bn as b,wr as c,yn as d,En as e,Ni as f,Bn as g,ji as h,$e as i,Qe as j,na as k,$i as l,Tr as m,Cn as n,Ta as o,Zi as p,_r as q,gr as r,_n as s};
//# sourceMappingURL=chunk-XGCT3WBO.js.map
