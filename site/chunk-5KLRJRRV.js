import{$a as pe,E as q,Ha as W,Ia as me,T as v,U as Y,_a as X,aa as I,fb as ue,ka as J,la as de,sb as Z,tb as he,ub as $,vb as be,wb as ee,xb as fe}from"./chunk-LNYDCYOQ.js";import{$c as G,Aa as h,Ca as y,Dc as x,Ea as a,Ec as M,Fc as w,Gc as k,Hb as c,Hc as B,Ic as F,Oc as D,Ta as A,Tc as H,Vc as _,Wc as R,Xc as d,Ya as j,Yb as l,Zb as b,Zc as L,_c as Q,_d as U,de as O,e as T,fc as N,fd as m,hb as z,id as S,ie as p,kd as E,ta as le,uc as C,wc as f,xd as K,yc as g}from"./chunk-NII6HHRB.js";import{e as u}from"./chunk-ID73HXZW.js";var te,ne=u(()=>{"use strict";p();pe();te=(()=>{class n{_animationsDisabled=X();state="unchecked";disabled=!1;appearance="full";constructor(){}static \u0275fac=function(o){return new(o||n)};static \u0275cmp=l({type:n,selectors:[["mat-pseudo-checkbox"]],hostAttrs:[1,"mat-pseudo-checkbox"],hostVars:12,hostBindings:function(o,t){o&2&&m("mat-pseudo-checkbox-indeterminate",t.state==="indeterminate")("mat-pseudo-checkbox-checked",t.state==="checked")("mat-pseudo-checkbox-disabled",t.disabled)("mat-pseudo-checkbox-minimal",t.appearance==="minimal")("mat-pseudo-checkbox-full",t.appearance==="full")("_mat-animation-noopable",t._animationsDisabled)},inputs:{state:"state",disabled:"disabled",appearance:"appearance"},decls:0,vars:0,template:function(o,t){},styles:[`.mat-pseudo-checkbox {
  border-radius: 2px;
  cursor: pointer;
  display: inline-block;
  vertical-align: middle;
  box-sizing: border-box;
  position: relative;
  flex-shrink: 0;
  transition: border-color 90ms cubic-bezier(0, 0, 0.2, 0.1), background-color 90ms cubic-bezier(0, 0, 0.2, 0.1);
}
.mat-pseudo-checkbox::after {
  position: absolute;
  opacity: 0;
  content: "";
  border-bottom: 2px solid currentColor;
  transition: opacity 90ms cubic-bezier(0, 0, 0.2, 0.1);
}
.mat-pseudo-checkbox._mat-animation-noopable {
  transition: none !important;
  animation: none !important;
}
.mat-pseudo-checkbox._mat-animation-noopable::after {
  transition: none;
}

.mat-pseudo-checkbox-disabled {
  cursor: default;
}

.mat-pseudo-checkbox-indeterminate::after {
  left: 1px;
  opacity: 1;
  border-radius: 2px;
}

.mat-pseudo-checkbox-checked::after {
  left: 1px;
  border-left: 2px solid currentColor;
  transform: rotate(-45deg);
  opacity: 1;
  box-sizing: content-box;
}

.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-checked::after, .mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-indeterminate::after {
  color: var(--mat-pseudo-checkbox-minimal-selected-checkmark-color, var(--mat-sys-primary));
}
.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-checked.mat-pseudo-checkbox-disabled::after, .mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-indeterminate.mat-pseudo-checkbox-disabled::after {
  color: var(--mat-pseudo-checkbox-minimal-disabled-selected-checkmark-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
}

.mat-pseudo-checkbox-full {
  border-color: var(--mat-pseudo-checkbox-full-unselected-icon-color, var(--mat-sys-on-surface-variant));
  border-width: 2px;
  border-style: solid;
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-disabled {
  border-color: var(--mat-pseudo-checkbox-full-disabled-unselected-icon-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate {
  background-color: var(--mat-pseudo-checkbox-full-selected-icon-color, var(--mat-sys-primary));
  border-color: transparent;
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked::after, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate::after {
  color: var(--mat-pseudo-checkbox-full-selected-checkmark-color, var(--mat-sys-on-primary));
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked.mat-pseudo-checkbox-disabled, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate.mat-pseudo-checkbox-disabled {
  background-color: var(--mat-pseudo-checkbox-full-disabled-selected-icon-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked.mat-pseudo-checkbox-disabled::after, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate.mat-pseudo-checkbox-disabled::after {
  color: var(--mat-pseudo-checkbox-full-disabled-selected-checkmark-color, var(--mat-sys-surface));
}

.mat-pseudo-checkbox {
  width: 18px;
  height: 18px;
}

.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-checked::after {
  width: 14px;
  height: 6px;
  transform-origin: center;
  top: -4.2426406871px;
  left: 0;
  bottom: 0;
  right: 0;
  margin: auto;
}
.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-indeterminate::after {
  top: 8px;
  width: 16px;
}

.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked::after {
  width: 10px;
  height: 4px;
  transform-origin: center;
  top: -2.8284271247px;
  left: 0;
  bottom: 0;
  right: 0;
  margin: auto;
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate::after {
  top: 6px;
  width: 12px;
}
`],encapsulation:2,changeDetection:0})}return n})()});function Me(n,i){if(n&1&&k(0,"mat-pseudo-checkbox",1),n&2){let e=_();x("disabled",e.disabled)("state",e.selected?"checked":"unchecked")}}function we(n,i){if(n&1&&k(0,"mat-pseudo-checkbox",3),n&2){let e=_();x("disabled",e.disabled)}}function De(n,i){if(n&1&&(M(0,"span",4),S(1),w()),n&2){let e=_();c(),E("(",e.group.label,")")}}function Ke(n,i,e){if(e.length){let o=i.toArray(),t=e.toArray(),r=0;for(let s=0;s<n+1;s++)o[s].group&&o[s].group===t[r]&&r++;return r}return 0}function Ue(n,i,e,o){return n<e?n:n+i>e+o?Math.max(0,n-o+i):e}var ke,_e,ve,ye,Ce,oe,ie,Ge,P,ae,re=u(()=>{"use strict";ue();de();p();p();le();he();ne();be();me();ke=["*",[["mat-option"],["ng-container"]]],_e=["*","mat-option, ng-container"],ve=["text"],ye=[[["mat-icon"]],"*"],Ce=["mat-icon","*"];oe=new y("MAT_OPTION_PARENT_COMPONENT"),ie=new y("MatOptgroup"),Ge=(()=>{class n{label;disabled=!1;_labelId=a(I).getId("mat-optgroup-label-");_inert;constructor(){let e=a(oe,{optional:!0});this._inert=e?.inertGroups??!1}static \u0275fac=function(o){return new(o||n)};static \u0275cmp=l({type:n,selectors:[["mat-optgroup"]],hostAttrs:[1,"mat-mdc-optgroup"],hostVars:3,hostBindings:function(o,t){o&2&&C("role",t._inert?null:"group")("aria-disabled",t._inert?null:t.disabled.toString())("aria-labelledby",t._inert?null:t._labelId)},inputs:{label:"label",disabled:[2,"disabled","disabled",O]},exportAs:["matOptgroup"],features:[K([{provide:ie,useExisting:n}])],ngContentSelectors:_e,decls:5,vars:4,consts:[["role","presentation",1,"mat-mdc-optgroup-label",3,"id"],[1,"mdc-list-item__primary-text"]],template:function(o,t){o&1&&(R(ke),B(0,"span",0)(1,"span",1),S(2),d(3),F()(),d(4,1)),o&2&&(m("mdc-list-item--disabled",t.disabled),D("id",t._labelId),c(2),E("",t.label," "))},styles:[`.mat-mdc-optgroup {
  color: var(--mat-optgroup-label-text-color, var(--mat-sys-on-surface-variant));
  font-family: var(--mat-optgroup-label-text-font, var(--mat-sys-title-small-font));
  line-height: var(--mat-optgroup-label-text-line-height, var(--mat-sys-title-small-line-height));
  font-size: var(--mat-optgroup-label-text-size, var(--mat-sys-title-small-size));
  letter-spacing: var(--mat-optgroup-label-text-tracking, var(--mat-sys-title-small-tracking));
  font-weight: var(--mat-optgroup-label-text-weight, var(--mat-sys-title-small-weight));
}

.mat-mdc-optgroup-label {
  display: flex;
  position: relative;
  align-items: center;
  justify-content: flex-start;
  overflow: hidden;
  min-height: 48px;
  padding: 0 16px;
  outline: none;
}
.mat-mdc-optgroup-label.mdc-list-item--disabled {
  opacity: 0.38;
}
.mat-mdc-optgroup-label .mdc-list-item__primary-text {
  font-size: inherit;
  font-weight: inherit;
  letter-spacing: inherit;
  line-height: inherit;
  font-family: inherit;
  text-decoration: inherit;
  text-transform: inherit;
  white-space: normal;
  color: inherit;
}
`],encapsulation:2,changeDetection:0})}return n})(),P=class{source;isUserInput;constructor(i,e=!1){this.source=i,this.isUserInput=e}},ae=(()=>{class n{_element=a(z);_changeDetectorRef=a(U);_parent=a(oe,{optional:!0});group=a(ie,{optional:!0});_signalDisableRipple=!1;_selected=!1;_active=!1;_mostRecentViewValue="";get multiple(){return this._parent&&this._parent.multiple}get selected(){return this._selected}value;id=a(I).getId("mat-option-");get disabled(){return this.group&&this.group.disabled||this._disabled()}set disabled(e){this._disabled.set(e)}_disabled=j(!1);get disableRipple(){return this._signalDisableRipple?this._parent.disableRipple():!!this._parent?.disableRipple}get hideSingleSelectionIndicator(){return!!(this._parent&&this._parent.hideSingleSelectionIndicator)}onSelectionChange=new A;_text;_stateChanges=new T;constructor(){let e=a(q);e.load($),e.load(W),this._signalDisableRipple=!!this._parent&&N(this._parent.disableRipple)}get active(){return this._active}get viewValue(){return(this._text?.nativeElement.textContent||"").trim()}select(e=!0){this._selected||(this._selected=!0,this._changeDetectorRef.markForCheck(),e&&this._emitSelectionChangeEvent())}deselect(e=!0){this._selected&&(this._selected=!1,this._changeDetectorRef.markForCheck(),e&&this._emitSelectionChangeEvent())}focus(e,o){let t=this._getHostElement();typeof t.focus=="function"&&t.focus(o)}setActiveStyles(){this._active||(this._active=!0,this._changeDetectorRef.markForCheck())}setInactiveStyles(){this._active&&(this._active=!1,this._changeDetectorRef.markForCheck())}getLabel(){return this.viewValue}_handleKeydown(e){(e.keyCode===13||e.keyCode===32)&&!J(e)&&(this._selectViaInteraction(),e.preventDefault())}_selectViaInteraction(){this.disabled||(this._selected=this.multiple?!this._selected:!0,this._changeDetectorRef.markForCheck(),this._emitSelectionChangeEvent(!0))}_getTabIndex(){return this.disabled?"-1":"0"}_getHostElement(){return this._element.nativeElement}ngAfterViewChecked(){if(this._selected){let e=this.viewValue;e!==this._mostRecentViewValue&&(this._mostRecentViewValue&&this._stateChanges.next(),this._mostRecentViewValue=e)}}ngOnDestroy(){this._stateChanges.complete()}_emitSelectionChangeEvent(e=!1){this.onSelectionChange.emit(new P(this,e))}static \u0275fac=function(o){return new(o||n)};static \u0275cmp=l({type:n,selectors:[["mat-option"]],viewQuery:function(o,t){if(o&1&&L(ve,7),o&2){let r;Q(r=G())&&(t._text=r.first)}},hostAttrs:["role","option",1,"mat-mdc-option","mdc-list-item"],hostVars:11,hostBindings:function(o,t){o&1&&H("click",function(){return t._selectViaInteraction()})("keydown",function(s){return t._handleKeydown(s)}),o&2&&(D("id",t.id),C("aria-selected",t.selected)("aria-disabled",t.disabled.toString()),m("mdc-list-item--selected",t.selected)("mat-mdc-option-multiple",t.multiple)("mat-mdc-option-active",t.active)("mdc-list-item--disabled",t.disabled))},inputs:{value:"value",id:"id",disabled:[2,"disabled","disabled",O]},outputs:{onSelectionChange:"onSelectionChange"},exportAs:["matOption"],ngContentSelectors:Ce,decls:8,vars:5,consts:[["text",""],["aria-hidden","true",1,"mat-mdc-option-pseudo-checkbox",3,"disabled","state"],[1,"mdc-list-item__primary-text"],["state","checked","aria-hidden","true","appearance","minimal",1,"mat-mdc-option-pseudo-checkbox",3,"disabled"],[1,"cdk-visually-hidden"],["aria-hidden","true","mat-ripple","",1,"mat-mdc-option-ripple","mat-focus-indicator",3,"matRippleTrigger","matRippleDisabled"]],template:function(o,t){o&1&&(R(ye),f(0,Me,1,2,"mat-pseudo-checkbox",1),d(1),M(2,"span",2,0),d(4,1),w(),f(5,we,1,1,"mat-pseudo-checkbox",3),f(6,De,2,1,"span",4),k(7,"div",5)),o&2&&(g(t.multiple?0:-1),c(5),g(!t.multiple&&t.selected&&!t.hideSingleSelectionIndicator?5:-1),c(),g(t.group&&t.group._inert?6:-1),c(),x("matRippleTrigger",t._getHostElement())("matRippleDisabled",t.disabled||t.disableRipple))},dependencies:[te,Z],styles:[`.mat-mdc-option {
  -webkit-user-select: none;
  user-select: none;
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  display: flex;
  position: relative;
  align-items: center;
  justify-content: flex-start;
  overflow: hidden;
  min-height: 48px;
  padding: 0 16px;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
  color: var(--mat-option-label-text-color, var(--mat-sys-on-surface));
  font-family: var(--mat-option-label-text-font, var(--mat-sys-label-large-font));
  line-height: var(--mat-option-label-text-line-height, var(--mat-sys-label-large-line-height));
  font-size: var(--mat-option-label-text-size, var(--mat-sys-body-large-size));
  letter-spacing: var(--mat-option-label-text-tracking, var(--mat-sys-label-large-tracking));
  font-weight: var(--mat-option-label-text-weight, var(--mat-sys-body-large-weight));
}
.mat-mdc-option:hover:not(.mdc-list-item--disabled) {
  background-color: var(--mat-option-hover-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-hover-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-option:focus.mdc-list-item, .mat-mdc-option.mat-mdc-option-active.mdc-list-item {
  background-color: var(--mat-option-focus-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-focus-state-layer-opacity) * 100%), transparent));
  outline: 0;
}
.mat-mdc-option.mdc-list-item--selected:not(.mdc-list-item--disabled):not(.mat-mdc-option-active, .mat-mdc-option-multiple, :focus, :hover) {
  background-color: var(--mat-option-selected-state-layer-color, var(--mat-sys-secondary-container));
}
.mat-mdc-option.mdc-list-item--selected:not(.mdc-list-item--disabled):not(.mat-mdc-option-active, .mat-mdc-option-multiple, :focus, :hover) .mdc-list-item__primary-text {
  color: var(--mat-option-selected-state-label-text-color, var(--mat-sys-on-secondary-container));
}
.mat-mdc-option .mat-pseudo-checkbox {
  --mat-pseudo-checkbox-minimal-selected-checkmark-color: var(--mat-option-selected-state-label-text-color, var(--mat-sys-on-secondary-container));
}
.mat-mdc-option.mdc-list-item {
  align-items: center;
  background: transparent;
}
.mat-mdc-option.mdc-list-item--disabled {
  cursor: default;
  pointer-events: none;
}
.mat-mdc-option.mdc-list-item--disabled .mat-mdc-option-pseudo-checkbox, .mat-mdc-option.mdc-list-item--disabled .mdc-list-item__primary-text, .mat-mdc-option.mdc-list-item--disabled > mat-icon {
  opacity: 0.38;
}
.mat-mdc-optgroup .mat-mdc-option:not(.mat-mdc-option-multiple) {
  padding-left: 32px;
}
[dir=rtl] .mat-mdc-optgroup .mat-mdc-option:not(.mat-mdc-option-multiple) {
  padding-left: 16px;
  padding-right: 32px;
}
.mat-mdc-option .mat-icon,
.mat-mdc-option .mat-pseudo-checkbox-full {
  margin-right: 16px;
  flex-shrink: 0;
}
[dir=rtl] .mat-mdc-option .mat-icon,
[dir=rtl] .mat-mdc-option .mat-pseudo-checkbox-full {
  margin-right: 0;
  margin-left: 16px;
}
.mat-mdc-option .mat-pseudo-checkbox-minimal {
  margin-left: 16px;
  flex-shrink: 0;
}
[dir=rtl] .mat-mdc-option .mat-pseudo-checkbox-minimal {
  margin-right: 16px;
  margin-left: 0;
}
.mat-mdc-option .mat-mdc-option-ripple {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
.mat-mdc-option .mdc-list-item__primary-text {
  white-space: normal;
  font-size: inherit;
  font-weight: inherit;
  letter-spacing: inherit;
  line-height: inherit;
  font-family: inherit;
  text-decoration: inherit;
  text-transform: inherit;
  margin-right: auto;
}
[dir=rtl] .mat-mdc-option .mdc-list-item__primary-text {
  margin-right: 0;
  margin-left: auto;
}
@media (forced-colors: active) {
  .mat-mdc-option.mdc-list-item--selected:not(:has(.mat-mdc-option-pseudo-checkbox))::after {
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
  [dir=rtl] .mat-mdc-option.mdc-list-item--selected:not(:has(.mat-mdc-option-pseudo-checkbox))::after {
    right: auto;
    left: 16px;
  }
}

.mat-mdc-option-multiple {
  --mat-list-list-item-selected-container-color: var(--mat-list-list-item-container-color, transparent);
}

.mat-mdc-option-active .mat-focus-indicator::before {
  content: "";
}
`],encapsulation:2,changeDetection:0})}return n})()});var se,ce=u(()=>{"use strict";Y();p();se=(()=>{class n{static \u0275fac=function(o){return new(o||n)};static \u0275mod=b({type:n});static \u0275inj=h({imports:[v]})}return n})()});var tt,Re=u(()=>{"use strict";Y();p();fe();ce();re();tt=(()=>{class n{static \u0275fac=function(o){return new(o||n)};static \u0275mod=b({type:n});static \u0275inj=h({imports:[ee,se,ae,v]})}return n})()});export{te as a,ne as b,oe as c,ie as d,Ge as e,P as f,ae as g,Ke as h,Ue as i,re as j,se as k,ce as l,tt as m,Re as n};
//# sourceMappingURL=chunk-5KLRJRRV.js.map
