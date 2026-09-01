import{e as s}from"./chunk-DPTWA756.js";var r,o,A,T,a=s(()=>{"use strict";r=new Map,o=()=>{let e=Date.now();for(let[t,n]of r.entries())e>n.expiresAt&&r.delete(t)},A=(e,t)=>{o(),r.set(t,{provider:e,expiresAt:Date.now()+6e5})},T=(e,t)=>{if(o(),!t)return!1;let n=r.get(t);return!n||n.provider!==e?!1:(r.delete(t),!0)}});export{A as a,T as b,a as c};
//# sourceMappingURL=chunk-RVYFOGBR.js.map
