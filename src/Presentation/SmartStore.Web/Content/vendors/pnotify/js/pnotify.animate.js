// Animate
!function(n,i){"function"==typeof define&&define.amd?define("pnotify.animate",["jquery","pnotify"],i):"object"==typeof exports&&"undefined"!=typeof module?module.exports=i(require("jquery"),require("./pnotify")):i(n.jQuery,n.PNotify)}("undefined"!=typeof window?window:this,function(n,i){return i.prototype.options.animate={animate:!1,in_class:"",out_class:""},i.prototype.modules.animate={init:function(n,i){this.setUpAnimations(n,i),n.attention=function(i,t){n.elem.one("webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend",function(){n.elem.removeClass(i),t&&t.call(n)}).addClass("animated "+i)}},update:function(n,i,t){i.animate!=t.animate&&this.setUpAnimations(n,i)},setUpAnimations:function(n,i){if(i.animate){n.options.animation="none",n.elem.removeClass("ui-pnotify-fade-slow ui-pnotify-fade-normal ui-pnotify-fade-fast"),n._animateIn||(n._animateIn=n.animateIn),n._animateOut||(n._animateOut=n.animateOut),n.animateIn=this.animateIn.bind(this),n.animateOut=this.animateOut.bind(this);var t=400;"slow"===n.options.animate_speed?t=600:"fast"===n.options.animate_speed?t=200:n.options.animate_speed>0&&(t=n.options.animate_speed),t/=1e3,n.elem.addClass("animated").css({"-webkit-animation-duration":t+"s","-moz-animation-duration":t+"s","animation-duration":t+"s"})}else n._animateIn&&n._animateOut&&(n.animateIn=n._animateIn,delete n._animateIn,n.animateOut=n._animateOut,delete n._animateOut,n.elem.addClass("animated"))},animateIn:function(n){this.notice.animating="in";var i=this;n=function(){i.notice.elem.removeClass(i.options.in_class),this&&this.call(),i.notice.animating=!1}.bind(n),this.notice.elem.show().one("webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend",n).removeClass(this.options.out_class).addClass("ui-pnotify-in").addClass(this.options.in_class)},animateOut:function(n){this.notice.animating="out";var i=this;n=function(){i.notice.elem.removeClass("ui-pnotify-in "+i.options.out_class),this&&this.call(),i.notice.animating=!1}.bind(n),this.notice.elem.one("webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend",n).removeClass(this.options.in_class).addClass(this.options.out_class)}},i});