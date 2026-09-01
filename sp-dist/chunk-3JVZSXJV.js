import{$ as lt,$a as ht,A as Se,Aa as ct,Ba as Ue,C as Oe,Ca as Ke,E as Ee,Ea as J,Ia as dt,L as Ae,T as Fe,U as rt,V as Be,X as Ne,Z as Le,_a as $,aa as He,bb as Ge,da as Z,fa as Ye,fb as pt,hb as _t,ja as mt,ka as Ve,la as ut,na as Xe,oa as je,ra as ze,sb as qe,tb as gt,ub as Ze,va as We,vb as ft,wb as Je,xb as bt,ya as Qe}from"./chunk-3EAF74DF.js";import{$c as v,Aa as me,Ca as _,Dc as Ce,Ea as o,Ec as W,F as w,Fc as Q,Gc as U,H as oe,Hb as Y,Hc as K,Ic as xe,La as g,Ma as f,Mb as pe,Na as ue,Nb as V,Nc as we,Oc as ke,Pa as b,Q as se,Qa as D,Rb as _e,Ta as m,Tb as ge,Tc as P,Ua as ce,Ub as X,Uc as De,Vc as c,Wc as G,Xc as R,Ya as de,Yb as j,Yc as Ie,Zb as fe,Zc as Pe,_b as M,_c as y,_d as C,a as x,bb as st,bc as z,dc as be,de as l,e as p,fd as T,hb as I,hd as Re,ib as he,ie as Te,ka as re,la as k,ma as H,na as le,o as L,oc as Me,ta as ot,uc as u,wc as ye,xd as q,yc as ve}from"./chunk-AHQARSDX.js";import{a as N,b as ae,e as at}from"./chunk-DPTWA756.js";function It(a,it){a&1&&(ue(),W(0,"svg",2),U(1,"polygon",3),Q())}function Rt(a,it){if(a&1){let e=we();K(0,"div",0),De("click",function(){g(e);let n=c();return f(n.closed.emit("click"))})("animationstart",function(n){g(e);let i=c();return f(i._onAnimationStart(n.animationName))})("animationend",function(n){g(e);let i=c();return f(i._onAnimationDone(n.animationName))})("animationcancel",function(n){g(e);let i=c();return f(i._onAnimationDone(n.animationName))}),K(1,"div",1),R(2),xe()()}if(a&2){let e=c();Re(e._classList),T("mat-menu-panel-animations-disabled",e._animationsDisabled)("mat-menu-panel-exit-animation",e._panelAnimationState==="void")("mat-menu-panel-animating",e._isAnimating()),ke("id",e.panelId),u("aria-label",e.ariaLabel||null)("aria-labelledby",e.ariaLabelledby||null)("aria-describedby",e.ariaDescribedby||null)}}var wt,kt,Dt,Pt,ne,te,tt,sn,Tt,ee,S,O,St,d,nt,rn,ln,mn,Ot=at(()=>{"use strict";mt();Te();Te();ot();pt();ut();st();dt();ft();gt();ht();rt();ct();_t();lt();bt();wt=["mat-menu-item",""],kt=[[["mat-icon"],["","matMenuItemIcon",""]],"*"],Dt=["mat-icon, [matMenuItemIcon]","*"];Pt=["*"];ne=new _("MAT_MENU_PANEL"),te=(()=>{class a{_elementRef=o(I);_document=o(D);_focusMonitor=o(J);_parentMenu=o(ne,{optional:!0});_changeDetectorRef=o(C);role="menuitem";disabled=!1;disableRipple=!1;_hovered=new p;_focused=new p;_highlighted=!1;_triggersSubmenu=!1;constructor(){o(Ee).load(Ze),this._parentMenu?.addItem?.(this)}focus(e,t){this._focusMonitor&&e?this._focusMonitor.focusVia(this._getHostElement(),e,t):this._getHostElement().focus(t),this._focused.next(this)}ngAfterViewInit(){this._focusMonitor&&this._focusMonitor.monitor(this._elementRef,!1)}ngOnDestroy(){this._focusMonitor&&this._focusMonitor.stopMonitoring(this._elementRef),this._parentMenu&&this._parentMenu.removeItem&&this._parentMenu.removeItem(this),this._hovered.complete(),this._focused.complete()}_getTabIndex(){return this.disabled?"-1":"0"}_getHostElement(){return this._elementRef.nativeElement}_checkDisabled(e){this.disabled&&(e.preventDefault(),e.stopPropagation())}_handleMouseEnter(){this._hovered.next(this)}getLabel(){let e=this._elementRef.nativeElement.cloneNode(!0),t=e.querySelectorAll("mat-icon, .material-icons");for(let n=0;n<t.length;n++)t[n].remove();return e.textContent?.trim()||""}_setHighlighted(e){this._highlighted=e,this._changeDetectorRef.markForCheck()}_setTriggersSubmenu(e){this._triggersSubmenu=e,this._changeDetectorRef.markForCheck()}_hasFocus(){return this._document&&this._document.activeElement===this._getHostElement()}static \u0275fac=function(t){return new(t||a)};static \u0275cmp=j({type:a,selectors:[["","mat-menu-item",""]],hostAttrs:[1,"mat-mdc-menu-item","mat-focus-indicator"],hostVars:8,hostBindings:function(t,n){t&1&&P("click",function(s){return n._checkDisabled(s)})("mouseenter",function(){return n._handleMouseEnter()}),t&2&&(u("role",n.role)("tabindex",n._getTabIndex())("aria-disabled",n.disabled)("disabled",n.disabled||null),T("mat-mdc-menu-item-highlighted",n._highlighted)("mat-mdc-menu-item-submenu-trigger",n._triggersSubmenu))},inputs:{role:"role",disabled:[2,"disabled","disabled",l],disableRipple:[2,"disableRipple","disableRipple",l]},exportAs:["matMenuItem"],attrs:wt,ngContentSelectors:Dt,decls:5,vars:3,consts:[[1,"mat-mdc-menu-item-text"],["matRipple","",1,"mat-mdc-menu-ripple",3,"matRippleDisabled","matRippleTrigger"],["viewBox","0 0 5 10","focusable","false","aria-hidden","true",1,"mat-mdc-menu-submenu-icon"],["points","0,0 5,5 0,10"]],template:function(t,n){t&1&&(G(kt),R(0),W(1,"span",0),R(2,1),Q(),U(3,"div",1),ye(4,It,2,0,":svg:svg",2)),t&2&&(Y(3),Ce("matRippleDisabled",n.disableRipple||n.disabled)("matRippleTrigger",n._getHostElement()),Y(),ve(n._triggersSubmenu?4:-1))},dependencies:[qe],encapsulation:2,changeDetection:0})}return a})(),tt=new _("MatMenuContent"),sn=(()=>{class a{_template=o(V);_appRef=o(Me);_injector=o(b);_viewContainerRef=o(X);_document=o(D);_changeDetectorRef=o(C);_portal;_outlet;_attached=new p;constructor(){}attach(e={}){this._portal||(this._portal=new Z(this._template,this._viewContainerRef)),this.detach(),this._outlet||(this._outlet=new Ye(this._document.createElement("div"),this._appRef,this._injector));let t=this._template.elementRef.nativeElement;t.parentNode.insertBefore(this._outlet.outletElement,t),this._changeDetectorRef.markForCheck(),this._portal.attach(this._outlet,e),this._attached.next()}detach(){this._portal?.isAttached&&this._portal.detach()}ngOnDestroy(){this.detach(),this._outlet?.dispose()}static \u0275fac=function(t){return new(t||a)};static \u0275dir=M({type:a,selectors:[["ng-template","matMenuContent",""]],features:[q([{provide:tt,useExisting:a}])]})}return a})(),Tt=new _("mat-menu-default-options",{providedIn:"root",factory:()=>({overlapTrigger:!1,xPosition:"after",yPosition:"below",backdropClass:"cdk-overlay-transparent-backdrop"})}),ee="_mat-menu-enter",S="_mat-menu-exit",O=(()=>{class a{_elementRef=o(I);_changeDetectorRef=o(C);_injector=o(b);_keyManager;_xPosition;_yPosition;_firstItemFocusRef;_exitFallbackTimeout;_animationsDisabled=$();_allItems;_directDescendantItems=new he;_classList={};_panelAnimationState="void";_animationDone=new p;_isAnimating=de(!1);parentMenu;direction;overlayPanelClass;backdropClass;ariaLabel;ariaLabelledby;ariaDescribedby;get xPosition(){return this._xPosition}set xPosition(e){this._xPosition=e,this.setPositionClasses()}get yPosition(){return this._yPosition}set yPosition(e){this._yPosition=e,this.setPositionClasses()}templateRef;items;lazyContent;overlapTrigger=!1;hasBackdrop;set panelClass(e){let t=this._previousPanelClass,n=N({},this._classList);t&&t.length&&t.split(" ").forEach(i=>{n[i]=!1}),this._previousPanelClass=e,e&&e.length&&(e.split(" ").forEach(i=>{n[i]=!0}),this._elementRef.nativeElement.className=""),this._classList=n}_previousPanelClass;get classList(){return this.panelClass}set classList(e){this.panelClass=e}closed=new m;close=this.closed;panelId=o(He).getId("mat-menu-panel-");constructor(){let e=o(Tt);this.overlayPanelClass=e.overlayPanelClass||"",this._xPosition=e.xPosition,this._yPosition=e.yPosition,this.backdropClass=e.backdropClass,this.overlapTrigger=e.overlapTrigger,this.hasBackdrop=e.hasBackdrop}ngOnInit(){this.setPositionClasses()}ngAfterContentInit(){this._updateDirectDescendants(),this._keyManager=new Ge(this._directDescendantItems).withWrap().withTypeAhead().withHomeAndEnd(),this._keyManager.tabOut.subscribe(()=>this.closed.emit("tab")),this._directDescendantItems.changes.pipe(k(this._directDescendantItems),H(e=>w(...e.map(t=>t._focused)))).subscribe(e=>this._keyManager.updateActiveItem(e)),this._directDescendantItems.changes.subscribe(e=>{let t=this._keyManager;if(this._panelAnimationState==="enter"&&t.activeItem?._hasFocus()){let n=e.toArray(),i=Math.max(0,Math.min(n.length-1,t.activeItemIndex||0));n[i]&&!n[i].disabled?t.setActiveItem(i):t.setNextItemActive()}})}ngOnDestroy(){this._keyManager?.destroy(),this._directDescendantItems.destroy(),this.closed.complete(),this._firstItemFocusRef?.destroy(),clearTimeout(this._exitFallbackTimeout)}_hovered(){return this._directDescendantItems.changes.pipe(k(this._directDescendantItems),H(t=>w(...t.map(n=>n._hovered))))}addItem(e){}removeItem(e){}_handleKeydown(e){let t=e.keyCode,n=this._keyManager;switch(t){case 27:Ve(e)||(e.preventDefault(),this.closed.emit("keydown"));break;case 37:this.parentMenu&&this.direction==="ltr"&&this.closed.emit("keydown");break;case 39:this.parentMenu&&this.direction==="rtl"&&this.closed.emit("keydown");break;default:(t===38||t===40)&&n.setFocusOrigin("keyboard"),n.onKeydown(e);return}}focusFirstItem(e="program"){this._firstItemFocusRef?.destroy(),this._firstItemFocusRef=pe(()=>{let t=this._resolvePanel();if(!t||!t.contains(document.activeElement)){let n=this._keyManager;n.setFocusOrigin(e).setFirstItemActive(),!n.activeItem&&t&&t.focus()}},{injector:this._injector})}resetActiveItem(){this._keyManager.setActiveItem(-1)}setElevation(e){}setPositionClasses(e=this.xPosition,t=this.yPosition){this._classList=ae(N({},this._classList),{"mat-menu-before":e==="before","mat-menu-after":e==="after","mat-menu-above":t==="above","mat-menu-below":t==="below"}),this._changeDetectorRef.markForCheck()}_onAnimationDone(e){let t=e===S;(t||e===ee)&&(t&&(clearTimeout(this._exitFallbackTimeout),this._exitFallbackTimeout=void 0),this._animationDone.next(t?"void":"enter"),this._isAnimating.set(!1))}_onAnimationStart(e){(e===ee||e===S)&&this._isAnimating.set(!0)}_setIsOpen(e){if(this._panelAnimationState=e?"enter":"void",e){if(this._keyManager.activeItemIndex===0){let t=this._resolvePanel();t&&(t.scrollTop=0)}}else this._animationsDisabled||(this._exitFallbackTimeout=setTimeout(()=>this._onAnimationDone(S),200));this._animationsDisabled&&setTimeout(()=>{this._onAnimationDone(e?ee:S)}),this._changeDetectorRef.markForCheck()}_updateDirectDescendants(){this._allItems.changes.pipe(k(this._allItems)).subscribe(e=>{this._directDescendantItems.reset(e.filter(t=>t._parentMenu===this)),this._directDescendantItems.notifyOnChanges()})}_resolvePanel(){let e=null;return this._directDescendantItems.length&&(e=this._directDescendantItems.first._getHostElement().closest('[role="menu"]')),e}static \u0275fac=function(t){return new(t||a)};static \u0275cmp=j({type:a,selectors:[["mat-menu"]],contentQueries:function(t,n,i){if(t&1&&Ie(i,tt,5)(i,te,5)(i,te,4),t&2){let s;y(s=v())&&(n.lazyContent=s.first),y(s=v())&&(n._allItems=s),y(s=v())&&(n.items=s)}},viewQuery:function(t,n){if(t&1&&Pe(V,5),t&2){let i;y(i=v())&&(n.templateRef=i.first)}},hostVars:3,hostBindings:function(t,n){t&2&&u("aria-label",null)("aria-labelledby",null)("aria-describedby",null)},inputs:{backdropClass:"backdropClass",ariaLabel:[0,"aria-label","ariaLabel"],ariaLabelledby:[0,"aria-labelledby","ariaLabelledby"],ariaDescribedby:[0,"aria-describedby","ariaDescribedby"],xPosition:"xPosition",yPosition:"yPosition",overlapTrigger:[2,"overlapTrigger","overlapTrigger",l],hasBackdrop:[2,"hasBackdrop","hasBackdrop",e=>e==null?null:l(e)],panelClass:[0,"class","panelClass"],classList:"classList"},outputs:{closed:"closed",close:"close"},exportAs:["matMenu"],features:[q([{provide:ne,useExisting:a}])],ngContentSelectors:Pt,decls:1,vars:0,consts:[["tabindex","-1","role","menu",1,"mat-mdc-menu-panel",3,"click","animationstart","animationend","animationcancel","id"],[1,"mat-mdc-menu-content"]],template:function(t,n){t&1&&(G(),be(0,Rt,3,12,"ng-template"))},styles:[`mat-menu {
  display: none;
}

.mat-mdc-menu-content {
  margin: 0;
  padding: 8px 0;
  outline: 0;
}
.mat-mdc-menu-content,
.mat-mdc-menu-content .mat-mdc-menu-item .mat-mdc-menu-item-text {
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  flex: 1;
  white-space: normal;
  font-family: var(--mat-menu-item-label-text-font, var(--mat-sys-label-large-font));
  line-height: var(--mat-menu-item-label-text-line-height, var(--mat-sys-label-large-line-height));
  font-size: var(--mat-menu-item-label-text-size, var(--mat-sys-label-large-size));
  letter-spacing: var(--mat-menu-item-label-text-tracking, var(--mat-sys-label-large-tracking));
  font-weight: var(--mat-menu-item-label-text-weight, var(--mat-sys-label-large-weight));
}

@keyframes _mat-menu-enter {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes _mat-menu-exit {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}
.mat-mdc-menu-panel {
  min-width: 112px;
  max-width: 280px;
  overflow: auto;
  box-sizing: border-box;
  outline: 0;
  animation: _mat-menu-enter 120ms cubic-bezier(0, 0, 0.2, 1);
  border-radius: var(--mat-menu-container-shape, var(--mat-sys-corner-extra-small));
  background-color: var(--mat-menu-container-color, var(--mat-sys-surface-container));
  box-shadow: var(--mat-menu-container-elevation-shadow, 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14), 0px 1px 5px 0px rgba(0, 0, 0, 0.12));
  will-change: transform, opacity;
}
.mat-mdc-menu-panel.mat-menu-panel-exit-animation {
  animation: _mat-menu-exit 100ms 25ms linear forwards;
}
.mat-mdc-menu-panel.mat-menu-panel-animations-disabled {
  animation: none;
}
.mat-mdc-menu-panel.mat-menu-panel-animating {
  pointer-events: none;
}
.mat-mdc-menu-panel.mat-menu-panel-animating:has(.mat-mdc-menu-content:empty) {
  display: none;
}
@media (forced-colors: active) {
  .mat-mdc-menu-panel {
    outline: solid 1px;
  }
}
.mat-mdc-menu-panel .mat-divider {
  border-top-color: var(--mat-menu-divider-color, var(--mat-sys-surface-variant));
  margin-bottom: var(--mat-menu-divider-bottom-spacing, 8px);
  margin-top: var(--mat-menu-divider-top-spacing, 8px);
}

.mat-mdc-menu-item {
  display: flex;
  position: relative;
  align-items: center;
  justify-content: flex-start;
  overflow: hidden;
  padding: 0;
  cursor: pointer;
  width: 100%;
  text-align: left;
  box-sizing: border-box;
  color: inherit;
  font-size: inherit;
  background: none;
  text-decoration: none;
  margin: 0;
  min-height: 48px;
  padding-left: var(--mat-menu-item-leading-spacing, 12px);
  padding-right: var(--mat-menu-item-trailing-spacing, 12px);
  -webkit-user-select: none;
  user-select: none;
  cursor: pointer;
  outline: none;
  border: none;
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-menu-item::-moz-focus-inner {
  border: 0;
}
[dir=rtl] .mat-mdc-menu-item {
  padding-left: var(--mat-menu-item-trailing-spacing, 12px);
  padding-right: var(--mat-menu-item-leading-spacing, 12px);
}
.mat-mdc-menu-item:has(.material-icons, mat-icon, [matButtonIcon]) {
  padding-left: var(--mat-menu-item-with-icon-leading-spacing, 12px);
  padding-right: var(--mat-menu-item-with-icon-trailing-spacing, 12px);
}
[dir=rtl] .mat-mdc-menu-item:has(.material-icons, mat-icon, [matButtonIcon]) {
  padding-left: var(--mat-menu-item-with-icon-trailing-spacing, 12px);
  padding-right: var(--mat-menu-item-with-icon-leading-spacing, 12px);
}
.mat-mdc-menu-item, .mat-mdc-menu-item:visited, .mat-mdc-menu-item:link {
  color: var(--mat-menu-item-label-text-color, var(--mat-sys-on-surface));
}
.mat-mdc-menu-item .mat-icon-no-color,
.mat-mdc-menu-item .mat-mdc-menu-submenu-icon {
  color: var(--mat-menu-item-icon-color, var(--mat-sys-on-surface-variant));
}
.mat-mdc-menu-item[disabled] {
  cursor: default;
  opacity: 0.38;
}
.mat-mdc-menu-item[disabled]::after {
  display: block;
  position: absolute;
  content: "";
  top: 0;
  left: 0;
  bottom: 0;
  right: 0;
}
.mat-mdc-menu-item:focus {
  outline: 0;
}
.mat-mdc-menu-item .mat-icon {
  flex-shrink: 0;
  margin-right: var(--mat-menu-item-spacing, 12px);
  height: var(--mat-menu-item-icon-size, 24px);
  width: var(--mat-menu-item-icon-size, 24px);
}
[dir=rtl] .mat-mdc-menu-item {
  text-align: right;
}
[dir=rtl] .mat-mdc-menu-item .mat-icon {
  margin-right: 0;
  margin-left: var(--mat-menu-item-spacing, 12px);
}
.mat-mdc-menu-item:not([disabled]):hover {
  background-color: var(--mat-menu-item-hover-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-hover-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-menu-item:not([disabled]).cdk-program-focused, .mat-mdc-menu-item:not([disabled]).cdk-keyboard-focused, .mat-mdc-menu-item:not([disabled]).mat-mdc-menu-item-highlighted {
  background-color: var(--mat-menu-item-focus-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-focus-state-layer-opacity) * 100%), transparent));
}
@media (forced-colors: active) {
  .mat-mdc-menu-item {
    margin-top: 1px;
  }
}

.mat-mdc-menu-submenu-icon {
  width: var(--mat-menu-item-icon-size, 24px);
  height: 10px;
  fill: currentColor;
  padding-left: var(--mat-menu-item-spacing, 12px);
}
[dir=rtl] .mat-mdc-menu-submenu-icon {
  padding-right: var(--mat-menu-item-spacing, 12px);
  padding-left: 0;
}
[dir=rtl] .mat-mdc-menu-submenu-icon polygon {
  transform: scaleX(-1);
  transform-origin: center;
}
@media (forced-colors: active) {
  .mat-mdc-menu-submenu-icon {
    fill: CanvasText;
  }
}

.mat-mdc-menu-item .mat-mdc-menu-ripple {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
`],encapsulation:2,changeDetection:0})}return a})(),St=new _("mat-menu-scroll-strategy",{providedIn:"root",factory:()=>{let a=o(b);return()=>Xe(a)}}),d=new WeakMap,nt=(()=>{class a{_canHaveBackdrop;_element=o(I);_viewContainerRef=o(X);_menuItemInstance=o(te,{optional:!0,self:!0});_dir=o(Ae,{optional:!0});_focusMonitor=o(J);_ngZone=o(ce);_injector=o(b);_scrollStrategy=o(St);_changeDetectorRef=o(C);_animationsDisabled=$();_portal;_overlayRef=null;_menuOpen=!1;_closingActionsSubscription=x.EMPTY;_menuCloseSubscription=x.EMPTY;_pendingRemoval;_parentMaterialMenu;_parentInnerPadding;_openedBy=void 0;get _menu(){return this._menuInternal}set _menu(e){e!==this._menuInternal&&(this._menuInternal=e,this._menuCloseSubscription.unsubscribe(),e&&(this._parentMaterialMenu,this._menuCloseSubscription=e.close.subscribe(t=>{this._destroyMenu(t),(t==="click"||t==="tab")&&this._parentMaterialMenu&&this._parentMaterialMenu.closed.emit(t)})),this._menuItemInstance?._setTriggersSubmenu(this._triggersSubmenu()))}_menuInternal=null;constructor(e){this._canHaveBackdrop=e;let t=o(ne,{optional:!0});this._parentMaterialMenu=t instanceof O?t:void 0}ngOnDestroy(){this._menu&&this._ownsMenu(this._menu)&&d.delete(this._menu),this._pendingRemoval?.unsubscribe(),this._menuCloseSubscription.unsubscribe(),this._closingActionsSubscription.unsubscribe(),this._overlayRef&&(this._overlayRef.dispose(),this._overlayRef=null)}get menuOpen(){return this._menuOpen}get dir(){return this._dir&&this._dir.value==="rtl"?"rtl":"ltr"}_triggersSubmenu(){return!!(this._menuItemInstance&&this._parentMaterialMenu&&this._menu)}_closeMenu(){this._menu?.close.emit()}_openMenu(e){if(this._triggerIsAriaDisabled())return;let t=this._menu;if(this._menuOpen||!t)return;this._pendingRemoval?.unsubscribe();let n=d.get(t);d.set(t,this),n&&n!==this&&n._closeMenu();let i=this._createOverlay(t),s=i.getConfig(),r=s.positionStrategy;this._setPosition(t,r),this._canHaveBackdrop?s.hasBackdrop=t.hasBackdrop==null?!this._triggersSubmenu():t.hasBackdrop:s.hasBackdrop=t.hasBackdrop??!1,i.hasAttached()||(i.attach(this._getPortal(t)),t.lazyContent?.attach(this.menuData)),this._closingActionsSubscription=this._menuClosingActions().subscribe(()=>this._closeMenu()),t.parentMenu=this._triggersSubmenu()?this._parentMaterialMenu:void 0,t.direction=this.dir,e&&t.focusFirstItem(this._openedBy||"program"),this._setIsMenuOpen(!0),t instanceof O&&(t._setIsOpen(!0),t._directDescendantItems.changes.pipe(le(t.close)).subscribe(()=>{r.withLockedPosition(!1).reapplyLastPosition(),r.withLockedPosition(!0)}))}focus(e,t){this._focusMonitor&&e?this._focusMonitor.focusVia(this._element,e,t):this._element.nativeElement.focus(t)}_destroyMenu(e){let t=this._overlayRef,n=this._menu;!t||!this.menuOpen||(this._closingActionsSubscription.unsubscribe(),this._pendingRemoval?.unsubscribe(),n instanceof O&&this._ownsMenu(n)?(this._pendingRemoval=n._animationDone.pipe(se(1)).subscribe(()=>{t.detach(),d.has(n)||n.lazyContent?.detach()}),n._setIsOpen(!1)):(t.detach(),n?.lazyContent?.detach()),n&&this._ownsMenu(n)&&d.delete(n),this.restoreFocus&&(e==="keydown"||!this._openedBy||!this._triggersSubmenu())&&this.focus(this._openedBy),this._openedBy=void 0,this._setIsMenuOpen(!1))}_setIsMenuOpen(e){e!==this._menuOpen&&(this._menuOpen=e,this._menuOpen?this.menuOpened.emit():this.menuClosed.emit(),this._triggersSubmenu()&&this._menuItemInstance._setHighlighted(e),this._changeDetectorRef.markForCheck())}_createOverlay(e){if(!this._overlayRef){let t=this._getOverlayConfig(e);this._subscribeToPositions(e,t.positionStrategy),this._overlayRef=We(this._injector,t),this._overlayRef.keydownEvents().subscribe(n=>{this._menu instanceof O&&this._menu._handleKeydown(n)})}return this._overlayRef}_getOverlayConfig(e){return new je({positionStrategy:ze(this._injector,this._getOverlayOrigin()).withLockedPosition().withGrowAfterOpen().withTransformOriginOn(".mat-menu-panel, .mat-mdc-menu-panel"),backdropClass:e.backdropClass||"cdk-overlay-transparent-backdrop",panelClass:e.overlayPanelClass,scrollStrategy:this._scrollStrategy(),direction:this._dir||"ltr",disableAnimations:this._animationsDisabled})}_subscribeToPositions(e,t){e.setPositionClasses&&t.positionChanges.subscribe(n=>{this._ngZone.run(()=>{let i=n.connectionPair.overlayX==="start"?"after":"before",s=n.connectionPair.overlayY==="top"?"below":"above";e.setPositionClasses(i,s)})})}_setPosition(e,t){let[n,i]=e.xPosition==="before"?["end","start"]:["start","end"],[s,r]=e.yPosition==="above"?["bottom","top"]:["top","bottom"],[E,A]=[s,r],[F,B]=[n,i],h=0;if(this._triggersSubmenu()){if(B=n=e.xPosition==="before"?"start":"end",i=F=n==="end"?"start":"end",this._parentMaterialMenu){if(this._parentInnerPadding==null){let ie=this._parentMaterialMenu.items.first;this._parentInnerPadding=ie?ie._getHostElement().offsetTop:0}h=s==="bottom"?this._parentInnerPadding:-this._parentInnerPadding}}else e.overlapTrigger||(E=s==="top"?"bottom":"top",A=r==="top"?"bottom":"top");t.withPositions([{originX:n,originY:E,overlayX:F,overlayY:s,offsetY:h},{originX:i,originY:E,overlayX:B,overlayY:s,offsetY:h},{originX:n,originY:A,overlayX:F,overlayY:r,offsetY:-h},{originX:i,originY:A,overlayX:B,overlayY:r,offsetY:-h}])}_menuClosingActions(){let e=this._getOutsideClickStream(this._overlayRef),t=this._overlayRef.detachments(),n=this._parentMaterialMenu?this._parentMaterialMenu.closed:L(),i=this._parentMaterialMenu?this._parentMaterialMenu._hovered().pipe(oe(s=>this._menuOpen&&s!==this._menuItemInstance)):L();return w(e,n,i,t)}_getPortal(e){return(!this._portal||this._portal.templateRef!==e.templateRef)&&(this._portal=new Z(e.templateRef,this._viewContainerRef)),this._portal}_ownsMenu(e){return d.get(e)===this}_triggerIsAriaDisabled(){return l(this._element.nativeElement.getAttribute("aria-disabled"))}static \u0275fac=function(t){ge()};static \u0275dir=M({type:a})}return a})(),rn=(()=>{class a extends nt{_cleanupTouchstart;_hoverSubscription=x.EMPTY;get _deprecatedMatMenuTriggerFor(){return this.menu}set _deprecatedMatMenuTriggerFor(e){this.menu=e}get menu(){return this._menu}set menu(e){this._menu=e}menuData;restoreFocus=!0;menuOpened=new m;onMenuOpen=this.menuOpened;menuClosed=new m;onMenuClose=this.menuClosed;constructor(){super(!0);let e=o(_e);this._cleanupTouchstart=e.listen(this._element.nativeElement,"touchstart",t=>{Ke(t)||(this._openedBy="touch")},{passive:!0})}triggersSubmenu(){return super._triggersSubmenu()}toggleMenu(){return this.menuOpen?this.closeMenu():this.openMenu()}openMenu(){this._openMenu(!0)}closeMenu(){this._closeMenu()}updatePosition(){this._overlayRef?.updatePosition()}ngAfterContentInit(){this._handleHover()}ngOnDestroy(){super.ngOnDestroy(),this._cleanupTouchstart(),this._hoverSubscription.unsubscribe()}_getOverlayOrigin(){return this._element}_getOutsideClickStream(e){return e.backdropClick()}_handleMousedown(e){Ue(e)||(this._openedBy=e.button===0?"mouse":void 0,this.triggersSubmenu()&&e.preventDefault())}_handleKeydown(e){let t=e.keyCode;(t===13||t===32)&&(this._openedBy="keyboard"),this.triggersSubmenu()&&(t===39&&this.dir==="ltr"||t===37&&this.dir==="rtl")&&(this._openedBy="keyboard",this.openMenu())}_handleClick(e){this.triggersSubmenu()?(e.stopPropagation(),this.openMenu()):this.toggleMenu()}_handleHover(){this.triggersSubmenu()&&this._parentMaterialMenu&&(this._hoverSubscription=this._parentMaterialMenu._hovered().subscribe(e=>{e===this._menuItemInstance&&!e.disabled&&this._parentMaterialMenu?._panelAnimationState!=="void"&&(this._openedBy="mouse",this._openMenu(!1))}))}static \u0275fac=function(t){return new(t||a)};static \u0275dir=M({type:a,selectors:[["","mat-menu-trigger-for",""],["","matMenuTriggerFor",""]],hostAttrs:[1,"mat-mdc-menu-trigger"],hostVars:3,hostBindings:function(t,n){t&1&&P("click",function(s){return n._handleClick(s)})("mousedown",function(s){return n._handleMousedown(s)})("keydown",function(s){return n._handleKeydown(s)}),t&2&&u("aria-haspopup",n.menu?"menu":null)("aria-expanded",n.menuOpen)("aria-controls",n.menuOpen?n.menu==null?null:n.menu.panelId:null)},inputs:{_deprecatedMatMenuTriggerFor:[0,"mat-menu-trigger-for","_deprecatedMatMenuTriggerFor"],menu:[0,"matMenuTriggerFor","menu"],menuData:[0,"matMenuTriggerData","menuData"],restoreFocus:[0,"matMenuTriggerRestoreFocus","restoreFocus"]},outputs:{menuOpened:"menuOpened",onMenuOpen:"onMenuOpen",menuClosed:"menuClosed",onMenuClose:"onMenuClose"},exportAs:["matMenuTrigger"],features:[z]})}return a})(),ln=(()=>{class a extends nt{_point={x:0,y:0,initialX:0,initialY:0,initialScrollX:0,initialScrollY:0};_triggerPressedControl=!1;_rootNode;_document=o(D);_viewportRuler=o(Ne);_scrollDispatcher=o(Be);_scrollSubscription;get menu(){return this._menu}set menu(e){this._menu=e}menuData;restoreFocus=!0;disabled=!1;menuOpened=new m;menuClosed=new m;constructor(){super(!1)}ngOnDestroy(){super.ngOnDestroy(),this._scrollSubscription?.unsubscribe()}_handleContextMenuEvent(e){this.disabled||(e.preventDefault(),this.menuOpen?(this._initializePoint(e.clientX,e.clientY),this._updatePosition()):this._openContextMenu(e))}_destroyMenu(e){super._destroyMenu(e),this._scrollSubscription?.unsubscribe()}_getOverlayOrigin(){return this._point}_getOutsideClickStream(e){return e.outsidePointerEvents().pipe(re((t,n)=>t.type==="contextmenu"?this._isWithinMenuOrTrigger(Oe(t)):t.type==="auxclick"?n===0?!0:(this._rootNode??=Se(this._element.nativeElement)||this._document,this._isWithinMenuOrTrigger(this._rootNode.elementFromPoint(t.clientX,t.clientY))):this._triggerPressedControl&&n===0&&t.ctrlKey))}_isWithinMenuOrTrigger(e){if(!e)return!1;let t=this._element.nativeElement;if(e===t||t.contains(e))return!0;let n=this._overlayRef?.hostElement;return n===e||!!n?.contains(e)}_openContextMenu(e){e.button===2?this._openedBy="mouse":this._openedBy=e.button===0?"keyboard":void 0,this._initializePoint(e.clientX,e.clientY),this._triggerPressedControl=e.ctrlKey,super._openMenu(!0),this._scrollSubscription?.unsubscribe(),this._scrollSubscription=this._scrollDispatcher.scrolled(0).subscribe(()=>{let t=this._viewportRuler.getViewportScrollPosition(),n=this._point;n.y=n.initialY+(n.initialScrollY-t.top),n.x=n.initialX+(n.initialScrollX-t.left),this._updatePosition()})}_initializePoint(e,t){let n=this._viewportRuler.getViewportScrollPosition(),i=this._point;i.x=i.initialX=e,i.y=i.initialY=t,i.initialScrollX=n.left,i.initialScrollY=n.top}_updatePosition(){let e=this._overlayRef;e&&(e.getConfig().positionStrategy.setOrigin(this._point),e.updatePosition())}static \u0275fac=function(t){return new(t||a)};static \u0275dir=M({type:a,selectors:[["","matContextMenuTriggerFor",""]],hostAttrs:[1,"mat-context-menu-trigger"],hostVars:3,hostBindings:function(t,n){t&1&&P("contextmenu",function(s){return n._handleContextMenuEvent(s)}),t&2&&(u("aria-controls",n.menuOpen?n.menu==null?null:n.menu.panelId:null),T("mat-context-menu-trigger-disabled",n.disabled))},inputs:{menu:[0,"matContextMenuTriggerFor","menu"],menuData:[0,"matContextMenuTriggerData","menuData"],restoreFocus:[0,"matContextMenuTriggerRestoreFocus","restoreFocus"],disabled:[2,"matContextMenuTriggerDisabled","disabled",l]},outputs:{menuOpened:"menuOpened",menuClosed:"menuClosed"},exportAs:["matContextMenuTrigger"],features:[z]})}return a})(),mn=(()=>{class a{static \u0275fac=function(t){return new(t||a)};static \u0275mod=fe({type:a});static \u0275inj=me({imports:[Je,Qe,Fe,Le]})}return a})()});export{te as a,sn as b,O as c,rn as d,ln as e,mn as f,Ot as g};
//# sourceMappingURL=chunk-3JVZSXJV.js.map
