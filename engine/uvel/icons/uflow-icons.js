/*!
 * UFlow Icons v1.1.0 — 98 animated gradient icons
 * MIT · https://uflow.uz/icons
 *
 * ── Plain HTML ──────────────────────────────────────────────────────
 *   <script src="https://uflow.uz/icons.js" defer></script>
 *   <u-icon name="bell" size="32" animated></u-icon>
 *
 * ── React (script tag, after React) ─────────────────────────────────
 *   const { Icon } = window.UFlowIcons;
 *   <Icon name="bell" size={32} animated />
 *
 * ── ES module ───────────────────────────────────────────────────────
 *   import { Icon, icons } from "https://uflow.uz/icons.js";
 *
 * Styles:  bulk (default) · stroke · solid · duotone · twotone
 * Corners: rounded (default) · standard · sharp
 */
(function (root, factory) {
  var api = factory();
  root.UFlowIcons = api;
  if (typeof module === "object" && module.exports) module.exports = api;
})(typeof self !== "undefined" ? self : this, function () {
  "use strict";

  var VERSION = "1.1.0";
  var ICONS = {"bell":[{"t":"path","a":{"d":"M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9z"},"m":"uicon-ring","o":"12px 3px"},{"t":"path","a":{"d":"M13.73 21a2 2 0 0 1-3.46 0"},"m":"uicon-ring","d":120,"o":"12px 17px"}],"search":[{"t":"circle","a":{"cx":11,"cy":11,"r":7},"m":"uicon-orbit"},{"t":"line","a":{"x1":16.5,"y1":16.5,"x2":21,"y2":21}}],"menu":[{"t":"line","a":{"x1":3,"y1":6,"x2":21,"y2":6},"m":"uicon-slide"},{"t":"line","a":{"x1":3,"y1":12,"x2":21,"y2":12},"m":"uicon-slide","d":120},{"t":"line","a":{"x1":3,"y1":18,"x2":21,"y2":18},"m":"uicon-slide","d":240}],"arrow-right":[{"t":"line","a":{"x1":4,"y1":12,"x2":19,"y2":12},"m":"uicon-slide"},{"t":"polyline","a":{"points":"13 6 19 12 13 18"},"m":"uicon-slide","d":60}],"settings":[{"t":"path","a":{"d":"M12 2.2l2 1.5 2.4-.7 1.1 2.2 2.4.6-.2 2.5 1.9 1.6-1.3 2.1 1 2.3-2.3 1-.5 2.4-2.5.1-1.6 1.9-2.1-1.3-2.4 1-1-2.3-2.4-.5-.1-2.5-1.9-1.6 1.3-2.1-1-2.3 2.3-1 .5-2.4 2.5-.1z"},"m":"uicon-spin","o":"12px 12px"},{"t":"circle","a":{"cx":12,"cy":12,"r":3}}],"sparkle":[{"t":"path","a":{"d":"M12 2.5l2.1 5.4 5.4 2.1-5.4 2.1L12 17.5l-2.1-5.4L4.5 10l5.4-2.1z"},"m":"uicon-pulse","o":"12px 10px","f":1},{"t":"path","a":{"d":"M19 16l.8 2.2 2.2.8-2.2.8L19 22l-.8-2.2-2.2-.8 2.2-.8z"},"m":"uicon-blink","d":200,"f":1},{"t":"path","a":{"d":"M5 15l.6 1.6 1.6.6-1.6.6L5 19.4l-.6-1.6L2.8 17.2l1.6-.6z"},"m":"uicon-blink","d":500,"f":1}],"chart":[{"t":"line","a":{"x1":3,"y1":21,"x2":21,"y2":21}},{"t":"line","a":{"x1":7,"y1":21,"x2":7,"y2":13},"m":"uicon-bounce","o":"7px 21px"},{"t":"line","a":{"x1":12,"y1":21,"x2":12,"y2":7},"m":"uicon-bounce","d":150,"o":"12px 21px"},{"t":"line","a":{"x1":17,"y1":21,"x2":17,"y2":10},"m":"uicon-bounce","d":300,"o":"17px 21px"}],"database":[{"t":"ellipse","a":{"cx":12,"cy":5.5,"rx":8,"ry":3},"m":"uicon-blink"},{"t":"path","a":{"d":"M4 5.5v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6"},"m":"uicon-blink","d":200},{"t":"path","a":{"d":"M4 11.5v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6"},"m":"uicon-blink","d":400}],"cpu":[{"t":"rect","a":{"x":5,"y":5,"width":14,"height":14,"rx":2.5}},{"t":"rect","a":{"x":9,"y":9,"width":6,"height":6,"rx":1},"m":"uicon-pulse","o":"12px 12px"},{"t":"line","a":{"x1":9,"y1":2,"x2":9,"y2":5},"m":"uicon-blink"},{"t":"line","a":{"x1":15,"y1":2,"x2":15,"y2":5},"m":"uicon-blink","d":150},{"t":"line","a":{"x1":9,"y1":19,"x2":9,"y2":22},"m":"uicon-blink","d":300},{"t":"line","a":{"x1":15,"y1":19,"x2":15,"y2":22},"m":"uicon-blink","d":450},{"t":"line","a":{"x1":2,"y1":9,"x2":5,"y2":9},"m":"uicon-blink","d":100},{"t":"line","a":{"x1":2,"y1":15,"x2":5,"y2":15},"m":"uicon-blink","d":250},{"t":"line","a":{"x1":19,"y1":9,"x2":22,"y2":9},"m":"uicon-blink","d":350},{"t":"line","a":{"x1":19,"y1":15,"x2":22,"y2":15},"m":"uicon-blink","d":500}],"layers":[{"t":"path","a":{"d":"M12 2.5L21 7l-9 4.5L3 7z"},"m":"uicon-bounce","f":1},{"t":"path","a":{"d":"M3 12l9 4.5L21 12"},"m":"uicon-bounce","d":120},{"t":"path","a":{"d":"M3 17l9 4.5L21 17"},"m":"uicon-bounce","d":240}],"code":[{"t":"polyline","a":{"points":"8 6 3 12 8 18"},"m":"uicon-slide","o":"5px 12px"},{"t":"polyline","a":{"points":"16 6 21 12 16 18"},"m":"uicon-slide","d":150,"o":"19px 12px"},{"t":"line","a":{"x1":13.5,"y1":5,"x2":10.5,"y2":19},"m":"uicon-blink","d":300}],"shield":[{"t":"path","a":{"d":"M12 2.5l8 3v6c0 5-3.4 9.3-8 10.5C7.4 20.8 4 16.5 4 11.5v-6z"},"m":"uicon-pulse","o":"12px 12px"},{"t":"polyline","a":{"points":"8.5 12 11 14.5 15.5 9.5"},"m":"uicon-draw"}],"check":[{"t":"circle","a":{"cx":12,"cy":12,"r":9},"m":"uicon-pulse","o":"12px 12px"},{"t":"polyline","a":{"points":"7.5 12.5 10.5 15.5 16.5 8.5"},"m":"uicon-draw"}],"zap":[{"t":"path","a":{"d":"M13.5 2L4 13.5h6.5L10 22l9.5-11.5H13z"},"m":"uicon-blink","f":1}],"heart":[{"t":"path","a":{"d":"M12 20.5l-1.5-1.3C5.4 14.6 2 11.6 2 7.9 2 5 4.2 2.9 7 2.9c1.6 0 3.1.7 4 1.9.9-1.2 2.4-1.9 4-1.9 2.8 0 5 2.1 5 5 0 3.7-3.4 6.7-8.5 11.3z"},"m":"uicon-beat","o":"12px 12px","f":1}],"clock":[{"t":"circle","a":{"cx":12,"cy":12,"r":9}},{"t":"line","a":{"x1":12,"y1":12,"x2":12,"y2":7},"m":"uicon-spin","o":"12px 12px"},{"t":"line","a":{"x1":12,"y1":12,"x2":15.5,"y2":13.5}}],"image":[{"t":"rect","a":{"x":3,"y":4,"width":18,"height":16,"rx":2.5}},{"t":"circle","a":{"cx":8.5,"cy":9.5,"r":1.8},"m":"uicon-bounce","o":"8.5px 9.5px","f":1},{"t":"polyline","a":{"points":"3.5 17 9 11.5 13 15.5 16 12.5 20.5 17"},"m":"uicon-draw"}],"send":[{"t":"path","a":{"d":"M21.5 2.5L2.5 10.5l7.5 3 3 7.5z"},"m":"uicon-slide","f":1},{"t":"line","a":{"x1":21.5,"y1":2.5,"x2":10,"y2":13.5},"m":"uicon-slide","d":60}],"globe":[{"t":"circle","a":{"cx":12,"cy":12,"r":9}},{"t":"ellipse","a":{"cx":12,"cy":12,"rx":4,"ry":9},"m":"uicon-wave","o":"12px 12px"},{"t":"line","a":{"x1":3,"y1":12,"x2":21,"y2":12}}],"rocket":[{"t":"path","a":{"d":"M12 2.5c3.5 2.5 5.5 6.5 5.5 10.5L12 17l-5.5-4c0-4 2-8 5.5-10.5z"},"m":"uicon-bounce"},{"t":"circle","a":{"cx":12,"cy":10,"r":2}},{"t":"path","a":{"d":"M9.5 17l-1.5 4.5L12 20l4 1.5-1.5-4.5"},"m":"uicon-blink","d":100,"f":1}],"mail":[{"t":"rect","a":{"x":2.5,"y":5,"width":19,"height":14,"rx":2.5}},{"t":"polyline","a":{"points":"3 7 12 13.5 21 7"},"m":"uicon-wave","o":"12px 7px"}],"users":[{"t":"circle","a":{"cx":9,"cy":8,"r":3.5},"m":"uicon-bounce"},{"t":"path","a":{"d":"M2.5 20a6.5 6.5 0 0 1 13 0"},"m":"uicon-bounce","d":80},{"t":"circle","a":{"cx":17.5,"cy":9,"r":2.8},"m":"uicon-bounce","d":200},{"t":"path","a":{"d":"M17.5 14.5a5 5 0 0 1 4 5.5"},"m":"uicon-bounce","d":280}],"server":[{"t":"rect","a":{"x":3,"y":4,"width":18,"height":7,"rx":2}},{"t":"rect","a":{"x":3,"y":13,"width":18,"height":7,"rx":2}},{"t":"circle","a":{"cx":7,"cy":7.5,"r":1},"m":"uicon-blink","f":1},{"t":"circle","a":{"cx":7,"cy":16.5,"r":1},"m":"uicon-blink","d":300,"f":1}],"file-text":[{"t":"path","a":{"d":"M14 2.5H6.5A1.5 1.5 0 0 0 5 4v16a1.5 1.5 0 0 0 1.5 1.5h11A1.5 1.5 0 0 0 19 20V7.5z"}},{"t":"polyline","a":{"points":"14 2.5 14 7.5 19 7.5"}},{"t":"line","a":{"x1":8.5,"y1":12,"x2":15.5,"y2":12},"m":"uicon-draw"},{"t":"line","a":{"x1":8.5,"y1":15.5,"x2":15.5,"y2":15.5},"m":"uicon-draw","d":150},{"t":"line","a":{"x1":8.5,"y1":19,"x2":12.5,"y2":19},"m":"uicon-draw","d":300}],"arrow-left":[{"t":"line","a":{"x1":20,"y1":12,"x2":5,"y2":12},"m":"uicon-slide"},{"t":"polyline","a":{"points":"11 6 5 12 11 18"},"m":"uicon-slide","d":60}],"arrow-up":[{"t":"line","a":{"x1":12,"y1":20,"x2":12,"y2":5},"m":"uicon-bounce"},{"t":"polyline","a":{"points":"6 11 12 5 18 11"},"m":"uicon-bounce","d":60}],"arrow-down":[{"t":"line","a":{"x1":12,"y1":4,"x2":12,"y2":19},"m":"uicon-bounce"},{"t":"polyline","a":{"points":"6 13 12 19 18 13"},"m":"uicon-bounce","d":60}],"refresh":[{"t":"path","a":{"d":"M20.5 12a8.5 8.5 0 1 1-2.5-6"},"m":"uicon-spin","o":"12px 12px"},{"t":"polyline","a":{"points":"18 2.5 18 6.5 14 6.5"},"m":"uicon-spin","o":"12px 12px"}],"external-link":[{"t":"path","a":{"d":"M14 3.5h6.5V10"},"m":"uicon-slide"},{"t":"line","a":{"x1":20.5,"y1":3.5,"x2":11,"y2":13},"m":"uicon-slide","d":60},{"t":"path","a":{"d":"M18 14.5v4A2.5 2.5 0 0 1 15.5 21h-10A2.5 2.5 0 0 1 3 18.5v-10A2.5 2.5 0 0 1 5.5 6h4"}}],"chevron-right":[{"t":"polyline","a":{"points":"9 5 16 12 9 19"},"m":"uicon-slide"}],"download":[{"t":"path","a":{"d":"M21 15.5v3A2.5 2.5 0 0 1 18.5 21h-13A2.5 2.5 0 0 1 3 18.5v-3"}},{"t":"polyline","a":{"points":"7 10 12 15 17 10"},"m":"uicon-bounce"},{"t":"line","a":{"x1":12,"y1":3,"x2":12,"y2":15},"m":"uicon-bounce","d":80}],"upload":[{"t":"path","a":{"d":"M21 15.5v3A2.5 2.5 0 0 1 18.5 21h-13A2.5 2.5 0 0 1 3 18.5v-3"}},{"t":"polyline","a":{"points":"7 8 12 3 17 8"},"m":"uicon-bounce"},{"t":"line","a":{"x1":12,"y1":3,"x2":12,"y2":15},"m":"uicon-bounce","d":80}],"plus":[{"t":"circle","a":{"cx":12,"cy":12,"r":9},"m":"uicon-pulse","o":"12px 12px"},{"t":"line","a":{"x1":12,"y1":8,"x2":12,"y2":16},"m":"uicon-blink"},{"t":"line","a":{"x1":8,"y1":12,"x2":16,"y2":12},"m":"uicon-blink","d":150}],"close":[{"t":"circle","a":{"cx":12,"cy":12,"r":9},"m":"uicon-pulse","o":"12px 12px"},{"t":"line","a":{"x1":8.5,"y1":8.5,"x2":15.5,"y2":15.5},"m":"uicon-draw"},{"t":"line","a":{"x1":15.5,"y1":8.5,"x2":8.5,"y2":15.5},"m":"uicon-draw","d":150}],"filter":[{"t":"line","a":{"x1":3,"y1":6,"x2":21,"y2":6},"m":"uicon-slide"},{"t":"line","a":{"x1":6,"y1":12,"x2":18,"y2":12},"m":"uicon-slide","d":120},{"t":"line","a":{"x1":9.5,"y1":18,"x2":14.5,"y2":18},"m":"uicon-slide","d":240}],"grid":[{"t":"rect","a":{"x":3,"y":3,"width":8,"height":8,"rx":2},"m":"uicon-blink"},{"t":"rect","a":{"x":13,"y":3,"width":8,"height":8,"rx":2},"m":"uicon-blink","d":150},{"t":"rect","a":{"x":3,"y":13,"width":8,"height":8,"rx":2},"m":"uicon-blink","d":450},{"t":"rect","a":{"x":13,"y":13,"width":8,"height":8,"rx":2},"m":"uicon-blink","d":300}],"star":[{"t":"path","a":{"d":"M12 2.5l2.9 6 6.6.9-4.8 4.6 1.2 6.5-5.9-3.1-5.9 3.1 1.2-6.5L2.5 9.4l6.6-.9z"},"m":"uicon-pulse","o":"12px 12px"}],"bookmark":[{"t":"path","a":{"d":"M6 3.5h12v18l-6-4.5-6 4.5z"},"m":"uicon-bounce","o":"12px 3px"}],"eye":[{"t":"path","a":{"d":"M1.5 12S5.5 5 12 5s10.5 7 10.5 7-4 7-10.5 7S1.5 12 1.5 12z"}},{"t":"circle","a":{"cx":12,"cy":12,"r":3.2},"m":"uicon-pulse","o":"12px 12px"}],"moon":[{"t":"path","a":{"d":"M20.5 14.5A8.5 8.5 0 0 1 9.5 3.5a8.5 8.5 0 1 0 11 11z"},"m":"uicon-wave","o":"12px 12px"}],"sun":[{"t":"circle","a":{"cx":12,"cy":12,"r":4.2},"m":"uicon-pulse","o":"12px 12px"},{"t":"line","a":{"x1":12,"y1":1.5,"x2":12,"y2":4},"m":"uicon-spin","o":"12px 12px"},{"t":"line","a":{"x1":12,"y1":20,"x2":12,"y2":22.5},"m":"uicon-spin","o":"12px 12px"},{"t":"line","a":{"x1":1.5,"y1":12,"x2":4,"y2":12},"m":"uicon-spin","o":"12px 12px"},{"t":"line","a":{"x1":20,"y1":12,"x2":22.5,"y2":12},"m":"uicon-spin","o":"12px 12px"}],"message":[{"t":"path","a":{"d":"M21 11.5a8.4 8.4 0 0 1-9 8.4 9 9 0 0 1-3.9-.9L3 20.5l1.5-4.6a8.4 8.4 0 0 1-.9-3.9 8.4 8.4 0 0 1 8.4-9 8.4 8.4 0 0 1 9 8.5z"}},{"t":"circle","a":{"cx":8.5,"cy":11.5,"r":1},"m":"uicon-blink","f":1},{"t":"circle","a":{"cx":12,"cy":11.5,"r":1},"m":"uicon-blink","d":200,"f":1},{"t":"circle","a":{"cx":15.5,"cy":11.5,"r":1},"m":"uicon-blink","d":400,"f":1}],"phone":[{"t":"path","a":{"d":"M21.5 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 1.6 4.2 2 2 0 0 1 3.6 2h3a2 2 0 0 1 2 1.7c.1 1 .4 1.9.7 2.8a2 2 0 0 1-.5 2.1L7.6 9.8a16 16 0 0 0 6 6l1.2-1.2a2 2 0 0 1 2.1-.5c.9.3 1.8.6 2.8.7a2 2 0 0 1 1.8 2.1z"},"m":"uicon-ring","o":"12px 12px"}],"share":[{"t":"circle","a":{"cx":18,"cy":5,"r":2.6},"m":"uicon-blink"},{"t":"circle","a":{"cx":6,"cy":12,"r":2.6},"m":"uicon-blink","d":200},{"t":"circle","a":{"cx":18,"cy":19,"r":2.6},"m":"uicon-blink","d":400},{"t":"line","a":{"x1":8.3,"y1":10.7,"x2":15.7,"y2":6.3}},{"t":"line","a":{"x1":8.3,"y1":13.3,"x2":15.7,"y2":17.7}}],"at-sign":[{"t":"circle","a":{"cx":12,"cy":12,"r":4},"m":"uicon-pulse","o":"12px 12px"},{"t":"path","a":{"d":"M16 8v5a3 3 0 0 0 6 0v-1a10 10 0 1 0-3.9 7.9"}}],"cart":[{"t":"path","a":{"d":"M2 3h3l2.7 12.4a2 2 0 0 0 2 1.6h8.6a2 2 0 0 0 2-1.5L22 7H6"}},{"t":"circle","a":{"cx":9.5,"cy":20,"r":1.5},"m":"uicon-spin","o":"9.5px 20px","f":1},{"t":"circle","a":{"cx":18,"cy":20,"r":1.5},"m":"uicon-spin","o":"18px 20px","f":1}],"wallet":[{"t":"path","a":{"d":"M20 7H5.5A2.5 2.5 0 0 1 5.5 2H19v5z"},"m":"uicon-slide"},{"t":"path","a":{"d":"M3 5v13a3 3 0 0 0 3 3h13a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2H6"}},{"t":"circle","a":{"cx":17,"cy":14,"r":1.4},"m":"uicon-pulse","o":"17px 14px","f":1}],"credit-card":[{"t":"rect","a":{"x":2,"y":5,"width":20,"height":14,"rx":2.5}},{"t":"line","a":{"x1":2,"y1":10,"x2":22,"y2":10},"m":"uicon-blink"},{"t":"line","a":{"x1":6,"y1":15,"x2":10,"y2":15},"m":"uicon-slide","d":150}],"gift":[{"t":"rect","a":{"x":3,"y":9,"width":18,"height":12,"rx":2},"m":"uicon-bounce"},{"t":"line","a":{"x1":12,"y1":9,"x2":12,"y2":21},"m":"uicon-blink"},{"t":"path","a":{"d":"M12 9C10 5.5 7 4 6 5.5S7.5 9 12 9c4.5 0 7-2 6-3.5S14 5.5 12 9z"},"m":"uicon-blink","d":200}],"tag":[{"t":"path","a":{"d":"M20.5 13.5l-7 7a2 2 0 0 1-2.8 0l-8-8V3.5H11l9.5 9.4a2 2 0 0 1 0 .6z"}},{"t":"circle","a":{"cx":7.5,"cy":7.5,"r":1.4},"m":"uicon-pulse","o":"7.5px 7.5px","f":1}],"folder":[{"t":"path","a":{"d":"M21 19a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h7a2 2 0 0 1 2 2z"},"m":"uicon-bounce"}],"trash":[{"t":"polyline","a":{"points":"3 6 21 6"},"m":"uicon-wave","o":"12px 6px"},{"t":"path","a":{"d":"M8 6V4a1.5 1.5 0 0 1 1.5-1.5h5A1.5 1.5 0 0 1 16 4v2"}},{"t":"path","a":{"d":"M18.5 6v13a2 2 0 0 1-2 2h-9a2 2 0 0 1-2-2V6"}},{"t":"line","a":{"x1":10,"y1":11,"x2":10,"y2":17},"m":"uicon-blink","d":150},{"t":"line","a":{"x1":14,"y1":11,"x2":14,"y2":17},"m":"uicon-blink","d":300}],"copy":[{"t":"rect","a":{"x":8,"y":8,"width":13,"height":13,"rx":2},"m":"uicon-slide"},{"t":"path","a":{"d":"M16 8V5a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h3"}}],"edit":[{"t":"path","a":{"d":"M17 3.5a2.8 2.8 0 0 1 4 4L7.5 21 2 22.5 3.5 17z"},"m":"uicon-wave","o":"12px 12px"},{"t":"line","a":{"x1":15,"y1":5.5,"x2":19,"y2":9.5},"m":"uicon-blink"}],"smartphone":[{"t":"rect","a":{"x":6,"y":2,"width":12,"height":20,"rx":2.5},"m":"uicon-pulse","o":"12px 12px"},{"t":"line","a":{"x1":10.5,"y1":18.5,"x2":13.5,"y2":18.5},"m":"uicon-blink"}],"monitor":[{"t":"rect","a":{"x":2,"y":3,"width":20,"height":13,"rx":2.5},"m":"uicon-pulse","o":"12px 9px"},{"t":"line","a":{"x1":8,"y1":21,"x2":16,"y2":21}},{"t":"line","a":{"x1":12,"y1":16,"x2":12,"y2":21}}],"wifi":[{"t":"path","a":{"d":"M2 8.5a16 16 0 0 1 20 0"},"m":"uicon-blink","d":400},{"t":"path","a":{"d":"M5 12.5a11 11 0 0 1 14 0"},"m":"uicon-blink","d":200},{"t":"path","a":{"d":"M8.5 16.3a6 6 0 0 1 7 0"},"m":"uicon-blink"},{"t":"circle","a":{"cx":12,"cy":20,"r":1.2},"m":"uicon-pulse","o":"12px 20px","f":1}],"battery":[{"t":"rect","a":{"x":2,"y":7,"width":17,"height":10,"rx":2.5}},{"t":"line","a":{"x1":22,"y1":10.5,"x2":22,"y2":13.5}},{"t":"rect","a":{"x":4.5,"y":9.5,"width":5,"height":5,"rx":1},"m":"uicon-blink","f":1},{"t":"rect","a":{"x":10.5,"y":9.5,"width":5,"height":5,"rx":1},"m":"uicon-blink","d":300,"f":1}],"cloud":[{"t":"path","a":{"d":"M17.5 19H7a5 5 0 0 1-.6-9.9 7 7 0 0 1 13.4 2.3A4.3 4.3 0 0 1 17.5 19z"},"m":"uicon-wave","o":"12px 13px"}],"cloud-rain":[{"t":"path","a":{"d":"M17.5 15H7a5 5 0 0 1-.6-9.9 7 7 0 0 1 13.4 2.3A4.3 4.3 0 0 1 17.5 15z"}},{"t":"line","a":{"x1":8.5,"y1":18,"x2":7.5,"y2":21},"m":"uicon-bounce"},{"t":"line","a":{"x1":12.5,"y1":18,"x2":11.5,"y2":21.5},"m":"uicon-bounce","d":200},{"t":"line","a":{"x1":16.5,"y1":18,"x2":15.5,"y2":21},"m":"uicon-bounce","d":400}],"flame":[{"t":"path","a":{"d":"M12 22c4.4 0 7-2.9 7-6.5 0-4.5-4-6-4-10.5C11 7 9 8.5 9 11c0-1-1-2-1-2-1.5 1.6-3 3.6-3 6.5C5 19.1 7.6 22 12 22z"},"m":"uicon-wave","o":"12px 20px"}],"droplet":[{"t":"path","a":{"d":"M12 2.5l5.7 6.9a7.4 7.4 0 1 1-11.4 0z"},"m":"uicon-bounce","o":"12px 12px"}],"user":[{"t":"circle","a":{"cx":12,"cy":8,"r":4},"m":"uicon-bounce"},{"t":"path","a":{"d":"M4 21a8 8 0 0 1 16 0"},"m":"uicon-bounce","d":80}],"award":[{"t":"circle","a":{"cx":12,"cy":8.5,"r":6},"m":"uicon-pulse","o":"12px 8.5px"},{"t":"polyline","a":{"points":"8.5 13.5 7 22 12 19 17 22 15.5 13.5"},"m":"uicon-wave","o":"12px 15px"}],"briefcase":[{"t":"rect","a":{"x":2,"y":7,"width":20,"height":14,"rx":2.5}},{"t":"path","a":{"d":"M8.5 7V5A2 2 0 0 1 10.5 3h3A2 2 0 0 1 15.5 5v2"},"m":"uicon-bounce"},{"t":"line","a":{"x1":2,"y1":12.5,"x2":22,"y2":12.5},"m":"uicon-blink","d":200}],"graduation":[{"t":"path","a":{"d":"M12 3L1.5 8.5 12 14l10.5-5.5z"},"m":"uicon-wave","o":"12px 8px"},{"t":"path","a":{"d":"M5.5 10.7V16c0 1.9 2.9 3.5 6.5 3.5s6.5-1.6 6.5-3.5v-5.3"},"m":"uicon-bounce","d":150}],"terminal":[{"t":"rect","a":{"x":2,"y":3.5,"width":20,"height":17,"rx":2.5}},{"t":"polyline","a":{"points":"6.5 9 10 12 6.5 15"},"m":"uicon-slide"},{"t":"line","a":{"x1":12.5,"y1":15.5,"x2":17.5,"y2":15.5},"m":"uicon-blink","d":200}],"git-branch":[{"t":"line","a":{"x1":6,"y1":5,"x2":6,"y2":15}},{"t":"circle","a":{"cx":6,"cy":18,"r":2.6},"m":"uicon-blink"},{"t":"circle","a":{"cx":6,"cy":3.5,"r":2.6},"m":"uicon-blink","d":200},{"t":"circle","a":{"cx":18,"cy":6,"r":2.6},"m":"uicon-blink","d":400},{"t":"path","a":{"d":"M18 8.6c0 4.4-4 5.4-8.6 6.4"}}],"bug":[{"t":"rect","a":{"x":7,"y":7,"width":10,"height":13,"rx":5},"m":"uicon-pulse","o":"12px 13px"},{"t":"path","a":{"d":"M9 6.5a3 3 0 0 1 6 0"}},{"t":"line","a":{"x1":3,"y1":11,"x2":7,"y2":12},"m":"uicon-wave","o":"7px 12px"},{"t":"line","a":{"x1":21,"y1":11,"x2":17,"y2":12},"m":"uicon-wave","o":"17px 12px"},{"t":"line","a":{"x1":3,"y1":18,"x2":7,"y2":16},"m":"uicon-wave","d":200,"o":"7px 16px"},{"t":"line","a":{"x1":21,"y1":18,"x2":17,"y2":16},"m":"uicon-wave","d":200,"o":"17px 16px"}],"package":[{"t":"path","a":{"d":"M21 8.5v7a2 2 0 0 1-1 1.7l-7 4a2 2 0 0 1-2 0l-7-4a2 2 0 0 1-1-1.7v-7a2 2 0 0 1 1-1.7l7-4a2 2 0 0 1 2 0l7 4a2 2 0 0 1 1 1.7z"},"m":"uicon-bounce"},{"t":"polyline","a":{"points":"3.3 7.5 12 12.5 20.7 7.5"},"m":"uicon-draw","d":150}],"key":[{"t":"circle","a":{"cx":7.5,"cy":16.5,"r":4.5},"m":"uicon-pulse","o":"7.5px 16.5px"},{"t":"line","a":{"x1":10.8,"y1":13.2,"x2":21,"y2":3},"m":"uicon-slide"},{"t":"line","a":{"x1":17,"y1":7,"x2":20,"y2":10},"m":"uicon-blink","d":200},{"t":"line","a":{"x1":14.5,"y1":9.5,"x2":17,"y2":12},"m":"uicon-blink","d":350}],"link":[{"t":"path","a":{"d":"M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7"},"m":"uicon-slide"},{"t":"path","a":{"d":"M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7"},"m":"uicon-slide","d":150}],"alert":[{"t":"path","a":{"d":"M10.3 3.4L1.8 17.5a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.4a2 2 0 0 0-3.4 0z"},"m":"uicon-pulse","o":"12px 14px"},{"t":"line","a":{"x1":12,"y1":9,"x2":12,"y2":13.5},"m":"uicon-blink"},{"t":"circle","a":{"cx":12,"cy":17,"r":1},"m":"uicon-blink","d":200,"f":1}],"info":[{"t":"circle","a":{"cx":12,"cy":12,"r":9},"m":"uicon-pulse","o":"12px 12px"},{"t":"line","a":{"x1":12,"y1":11,"x2":12,"y2":16.5},"m":"uicon-blink"},{"t":"circle","a":{"cx":12,"cy":7.8,"r":1},"m":"uicon-blink","d":200,"f":1}],"trending-up":[{"t":"polyline","a":{"points":"22 7 13.5 15.5 8.5 10.5 2 17"},"m":"uicon-draw"},{"t":"polyline","a":{"points":"16 7 22 7 22 13"},"m":"uicon-blink","d":400}],"pie-chart":[{"t":"path","a":{"d":"M21.2 15.9A10 10 0 1 1 8.1 2.8z"},"m":"uicon-pulse","o":"12px 12px"},{"t":"path","a":{"d":"M21.5 12A9.5 9.5 0 0 0 12 2.5V12z"},"m":"uicon-wave","o":"12px 12px","f":1}],"activity":[{"t":"polyline","a":{"points":"22 12 18 12 15 21 9 3 6 12 2 12"},"m":"uicon-draw"}],"lock":[{"t":"rect","a":{"x":4,"y":10.5,"width":16,"height":11,"rx":2.5},"m":"uicon-pulse","o":"12px 16px"},{"t":"path","a":{"d":"M8 10.5V7a4 4 0 0 1 8 0v3.5"},"m":"uicon-bounce"}],"target":[{"t":"circle","a":{"cx":12,"cy":12,"r":9.5},"m":"uicon-blink","d":400},{"t":"circle","a":{"cx":12,"cy":12,"r":5.8},"m":"uicon-blink","d":200},{"t":"circle","a":{"cx":12,"cy":12,"r":2.2},"m":"uicon-pulse","o":"12px 12px","f":1}],"play":[{"t":"circle","a":{"cx":12,"cy":12,"r":9.5},"m":"uicon-pulse","o":"12px 12px"},{"t":"path","a":{"d":"M10 8.2l6 3.8-6 3.8z"},"m":"uicon-slide","f":1}],"camera":[{"t":"path","a":{"d":"M22 18.5a2.5 2.5 0 0 1-2.5 2.5h-15A2.5 2.5 0 0 1 2 18.5v-11A2.5 2.5 0 0 1 4.5 5h3l2-3h5l2 3h3A2.5 2.5 0 0 1 22 7.5z"}},{"t":"circle","a":{"cx":12,"cy":13,"r":4},"m":"uicon-pulse","o":"12px 13px"}],"mic":[{"t":"rect","a":{"x":9,"y":2,"width":6,"height":12,"rx":3},"m":"uicon-pulse","o":"12px 8px"},{"t":"path","a":{"d":"M5 11a7 7 0 0 0 14 0"},"m":"uicon-blink","d":200},{"t":"line","a":{"x1":12,"y1":18,"x2":12,"y2":22}}],"volume":[{"t":"path","a":{"d":"M11 4.5L6.5 8.5H3v7h3.5L11 19.5z"},"m":"uicon-pulse","o":"7px 12px"},{"t":"path","a":{"d":"M15.5 9a4 4 0 0 1 0 6"},"m":"uicon-blink"},{"t":"path","a":{"d":"M18.5 6a8 8 0 0 1 0 12"},"m":"uicon-blink","d":250}],"map-pin":[{"t":"path","a":{"d":"M20 10c0 6-8 12.5-8 12.5S4 16 4 10a8 8 0 0 1 16 0z"},"m":"uicon-bounce"},{"t":"circle","a":{"cx":12,"cy":10,"r":3},"m":"uicon-pulse","o":"12px 10px"}],"calendar":[{"t":"rect","a":{"x":3,"y":5,"width":18,"height":16,"rx":2.5}},{"t":"line","a":{"x1":3,"y1":10,"x2":21,"y2":10}},{"t":"line","a":{"x1":8,"y1":2.5,"x2":8,"y2":6},"m":"uicon-bounce"},{"t":"line","a":{"x1":16,"y1":2.5,"x2":16,"y2":6},"m":"uicon-bounce","d":150},{"t":"circle","a":{"cx":8.5,"cy":14.5,"r":1.1},"m":"uicon-blink","f":1},{"t":"circle","a":{"cx":12.5,"cy":14.5,"r":1.1},"m":"uicon-blink","d":200,"f":1},{"t":"circle","a":{"cx":16.5,"cy":14.5,"r":1.1},"m":"uicon-blink","d":400,"f":1}],"utensils":[{"t":"path","a":{"d":"M6 2.5v7a2.5 2.5 0 0 0 5 0v-7"},"m":"uicon-wave","o":"8.5px 3px"},{"t":"line","a":{"x1":8.5,"y1":12,"x2":8.5,"y2":21.5}},{"t":"path","a":{"d":"M17.5 2.5c-1.7 1.3-2.5 3.3-2.5 5.5 0 1.9.8 3 2.5 3.5v10"},"m":"uicon-wave","d":200,"o":"16px 3px"}],"apple":[{"t":"path","a":{"d":"M12 7.5c-1-1-2.4-1.5-3.8-1.5C5.6 6 3.5 8.4 3.5 12c0 4.7 3.4 9.5 5.9 9.5 1 0 1.7-.5 2.6-.5s1.6.5 2.6.5c2.5 0 5.9-4.8 5.9-9.5 0-3.6-2.1-6-4.7-6-1.4 0-2.8.5-3.8 1.5z"},"m":"uicon-bounce"},{"t":"path","a":{"d":"M12 7.5V4.5a2.5 2.5 0 0 1 2.5-2.5"},"m":"uicon-wave","d":150,"o":"12px 7px"}],"dumbbell":[{"t":"rect","a":{"x":1.5,"y":8.5,"width":4,"height":7,"rx":1.5},"m":"uicon-bounce"},{"t":"rect","a":{"x":18.5,"y":8.5,"width":4,"height":7,"rx":1.5},"m":"uicon-bounce","d":150},{"t":"line","a":{"x1":5.5,"y1":12,"x2":18.5,"y2":12},"m":"uicon-pulse","o":"12px 12px"}],"coins":[{"t":"ellipse","a":{"cx":9,"cy":7.5,"rx":6.5,"ry":3},"m":"uicon-blink"},{"t":"path","a":{"d":"M2.5 7.5v4c0 1.7 2.9 3 6.5 3s6.5-1.3 6.5-3v-4"},"m":"uicon-blink","d":200},{"t":"path","a":{"d":"M8 14.4v2.1c0 1.7 2.9 3 6.5 3s6.5-1.3 6.5-3v-4c0-1.3-1.7-2.4-4.2-2.8"},"m":"uicon-blink","d":400}],"scale":[{"t":"rect","a":{"x":3,"y":3,"width":18,"height":18,"rx":4}},{"t":"path","a":{"d":"M8 13a4 4 0 0 1 8 0"},"m":"uicon-blink"},{"t":"line","a":{"x1":12,"y1":13,"x2":15,"y2":9.5},"m":"uicon-ring","o":"12px 13px"}],"log-in":[{"t":"path","a":{"d":"M9.5 3.5H18a2.5 2.5 0 0 1 2.5 2.5v12a2.5 2.5 0 0 1-2.5 2.5H9.5"}},{"t":"polyline","a":{"points":"9 16 13.5 12 9 8"},"m":"uicon-slide"},{"t":"line","a":{"x1":13.5,"y1":12,"x2":3,"y2":12},"m":"uicon-slide","d":60}],"log-out":[{"t":"path","a":{"d":"M14.5 3.5H6A2.5 2.5 0 0 0 3.5 6v12A2.5 2.5 0 0 0 6 20.5h8.5"}},{"t":"polyline","a":{"points":"16.5 16 21 12 16.5 8"},"m":"uicon-slide"},{"t":"line","a":{"x1":21,"y1":12,"x2":9.5,"y2":12},"m":"uicon-slide","d":60}],"palette":[{"t":"path","a":{"d":"M12 2.5a9.5 9.5 0 0 0 0 19c1 0 1.8-.8 1.8-1.8 0-.5-.2-.9-.5-1.2-.3-.3-.5-.7-.5-1.2 0-1 .8-1.8 1.8-1.8h2.1a4.8 4.8 0 0 0 4.8-4.8c0-4.5-4.3-8.2-9.5-8.2z"}},{"t":"circle","a":{"cx":7,"cy":11.5,"r":1.3},"m":"uicon-blink","f":1},{"t":"circle","a":{"cx":10,"cy":7,"r":1.3},"m":"uicon-blink","d":200,"f":1},{"t":"circle","a":{"cx":15,"cy":7.5,"r":1.3},"m":"uicon-blink","d":400,"f":1}],"sliders":[{"t":"line","a":{"x1":4,"y1":5,"x2":20,"y2":5}},{"t":"circle","a":{"cx":9,"cy":5,"r":2.4},"m":"uicon-slide","f":1},{"t":"line","a":{"x1":4,"y1":12,"x2":20,"y2":12}},{"t":"circle","a":{"cx":15,"cy":12,"r":2.4},"m":"uicon-slide","d":200,"f":1},{"t":"line","a":{"x1":4,"y1":19,"x2":20,"y2":19}},{"t":"circle","a":{"cx":7.5,"cy":19,"r":2.4},"m":"uicon-slide","d":400,"f":1}],"trending-down":[{"t":"polyline","a":{"points":"22 17 13.5 8.5 8.5 13.5 2 7"},"m":"uicon-draw"},{"t":"polyline","a":{"points":"16 17 22 17 22 11"},"m":"uicon-blink","d":400}],"bot":[{"t":"rect","a":{"x":3.5,"y":8,"width":17,"height":12,"rx":3},"m":"uicon-pulse","o":"12px 14px"},{"t":"line","a":{"x1":12,"y1":3,"x2":12,"y2":8},"m":"uicon-ring","o":"12px 8px"},{"t":"circle","a":{"cx":12,"cy":2.5,"r":1.4},"m":"uicon-blink","f":1},{"t":"circle","a":{"cx":8.5,"cy":13.5,"r":1.3},"m":"uicon-blink","d":200,"f":1},{"t":"circle","a":{"cx":15.5,"cy":13.5,"r":1.3},"m":"uicon-blink","d":200,"f":1}],"wifi-off":[{"t":"path","a":{"d":"M8.5 16.3a6 6 0 0 1 7 0"},"m":"uicon-blink"},{"t":"path","a":{"d":"M5 12.5a11 11 0 0 1 4-2.4"},"m":"uicon-blink","d":200},{"t":"path","a":{"d":"M15 10.1a11 11 0 0 1 4 2.4"},"m":"uicon-blink","d":200},{"t":"circle","a":{"cx":12,"cy":20,"r":1.2},"f":1},{"t":"line","a":{"x1":2.5,"y1":2.5,"x2":21.5,"y2":21.5},"m":"uicon-draw"}],"badge-check":[{"t":"path","a":{"d":"M12 2.2l2.5 2 3.2-.2.9 3 2.6 1.9-1.3 2.9 1.3 2.9-2.6 1.9-.9 3-3.2-.2-2.5 2-2.5-2-3.2.2-.9-3L2.8 14.7l1.3-2.9-1.3-2.9L5.4 7l.9-3 3.2.2z"},"m":"uicon-pulse","o":"12px 12px"},{"t":"polyline","a":{"points":"8.5 12 11 14.5 15.5 9.5"},"m":"uicon-draw"}]};
  var GRADIENTS = {"uflow":["#2563eb","#a21cf0","#06d64a"],"ocean":["#00c2ff","#2563eb","#6d28d9"],"sunset":["#ffa800","#ff2d55","#ff00a8"],"forest":["#00e05a","#00b894","#0891b2"],"candy":["#ff2d9b","#8b1eff"],"gold":["#ffd000","#ff8a00"],"green":["#4ade80","#22c55e","#15803d"],"neon":["#00ffa3","#00d1ff","#8b5cf6"],"fire":["#ffdd00","#ff5e00","#e60000"],"mono":["currentColor","currentColor"]};
  var CORNERS = {"rounded":{"cap":"round","join":"round","miter":4},"standard":{"cap":"square","join":"bevel","miter":4},"sharp":{"cap":"butt","join":"miter","miter":10}};
  var NS = "http://www.w3.org/2000/svg";
  var CSS = "/* ==========================================================================\n   UFlow Icons — motion stylesheet\n   v1.0 · MIT\n\n   Works with both the React component and the plain-HTML sprite. Every\n   animation touches only transform/opacity (plus stroke-dashoffset for the\n   draw effect) so it stays on the compositor.\n\n   Usage in plain HTML:\n     <link rel=\"stylesheet\" href=\"/uflow-icons.css\">\n     <svg class=\"uicon uicon-animated\"> ... </svg>\n   ========================================================================== */\n\n.uicon {\n  display: inline-block;\n  vertical-align: middle;\n  overflow: visible;\n  flex-shrink: 0;\n}\n\n/* Motion is opt-in: it runs when the SVG carries .uicon-animated, or when an\n   ancestor .group is hovered and the SVG carries .uicon-on-hover. */\n.uicon-animated .uicon-ring,\n.uicon-animated .uicon-pulse,\n.uicon-animated .uicon-spin,\n.uicon-animated .uicon-orbit,\n.uicon-animated .uicon-bounce,\n.uicon-animated .uicon-draw,\n.uicon-animated .uicon-blink,\n.uicon-animated .uicon-slide,\n.uicon-animated .uicon-wave,\n.uicon-animated .uicon-beat,\n.group:hover .uicon-on-hover .uicon-ring,\n.group:hover .uicon-on-hover .uicon-pulse,\n.group:hover .uicon-on-hover .uicon-spin,\n.group:hover .uicon-on-hover .uicon-orbit,\n.group:hover .uicon-on-hover .uicon-bounce,\n.group:hover .uicon-on-hover .uicon-draw,\n.group:hover .uicon-on-hover .uicon-blink,\n.group:hover .uicon-on-hover .uicon-slide,\n.group:hover .uicon-on-hover .uicon-wave,\n.group:hover .uicon-on-hover .uicon-beat {\n  animation-duration: var(--uicon-duration, 2s);\n  animation-timing-function: var(--uicon-ease, cubic-bezier(0.4, 0, 0.2, 1));\n  animation-iteration-count: infinite;\n  animation-fill-mode: both;\n}\n\n.uicon-animated .uicon-ring,\n.group:hover .uicon-on-hover .uicon-ring { animation-name: uiconRing; }\n\n.uicon-animated .uicon-pulse,\n.group:hover .uicon-on-hover .uicon-pulse { animation-name: uiconPulse; }\n\n.uicon-animated .uicon-spin,\n.group:hover .uicon-on-hover .uicon-spin {\n  animation-name: uiconSpin;\n  animation-duration: var(--uicon-duration, 6s);\n  animation-timing-function: linear;\n}\n\n.uicon-animated .uicon-orbit,\n.group:hover .uicon-on-hover .uicon-orbit {\n  animation-name: uiconOrbit;\n  animation-duration: var(--uicon-duration, 3s);\n  animation-timing-function: ease-in-out;\n}\n\n.uicon-animated .uicon-bounce,\n.group:hover .uicon-on-hover .uicon-bounce { animation-name: uiconBounce; }\n\n.uicon-animated .uicon-blink,\n.group:hover .uicon-on-hover .uicon-blink { animation-name: uiconBlink; }\n\n.uicon-animated .uicon-slide,\n.group:hover .uicon-on-hover .uicon-slide { animation-name: uiconSlide; }\n\n.uicon-animated .uicon-wave,\n.group:hover .uicon-on-hover .uicon-wave { animation-name: uiconWave; }\n\n.uicon-animated .uicon-beat,\n.group:hover .uicon-on-hover .uicon-beat {\n  animation-name: uiconBeat;\n  animation-duration: var(--uicon-duration, 1.4s);\n}\n\n.uicon-animated .uicon-draw,\n.group:hover .uicon-on-hover .uicon-draw {\n  stroke-dasharray: 48;\n  animation-name: uiconDraw;\n  animation-duration: var(--uicon-duration, 2.4s);\n  animation-timing-function: ease-in-out;\n}\n\n/* ── Keyframes ─────────────────────────────────────────────────────── */\n\n/* Bell: two quick swings, then rest. */\n@keyframes uiconRing {\n  0%, 55%, 100% { transform: rotate(0deg); }\n  60%  { transform: rotate(11deg); }\n  67%  { transform: rotate(-9deg); }\n  74%  { transform: rotate(6deg); }\n  81%  { transform: rotate(-4deg); }\n  88%  { transform: rotate(2deg); }\n}\n\n@keyframes uiconPulse {\n  0%, 100% { transform: scale(1); }\n  50%      { transform: scale(1.11); }\n}\n\n@keyframes uiconSpin {\n  to { transform: rotate(360deg); }\n}\n\n/* Small circular travel — the lens sweep on the search icon. */\n@keyframes uiconOrbit {\n  0%, 100% { transform: translate(0, 0); }\n  25%      { transform: translate(1px, -1px); }\n  50%      { transform: translate(0, -1.5px); }\n  75%      { transform: translate(-1px, -1px); }\n}\n\n@keyframes uiconBounce {\n  0%, 100% { transform: translateY(0); }\n  40%      { transform: translateY(-2.2px); }\n}\n\n@keyframes uiconBlink {\n  0%, 100% { opacity: 1; }\n  45%      { opacity: 0.25; }\n}\n\n@keyframes uiconSlide {\n  0%, 100% { transform: translateX(0); }\n  50%      { transform: translateX(2px); }\n}\n\n@keyframes uiconWave {\n  0%, 100% { transform: skewX(0deg) scaleX(1); }\n  50%      { transform: skewX(-5deg) scaleX(0.94); }\n}\n\n/* Heartbeat: thump-thump, pause. */\n@keyframes uiconBeat {\n  0%, 100% { transform: scale(1); }\n  14%      { transform: scale(1.16); }\n  28%      { transform: scale(1); }\n  42%      { transform: scale(1.11); }\n  56%      { transform: scale(1); }\n}\n\n@keyframes uiconDraw {\n  0%   { stroke-dashoffset: 48; }\n  45%  { stroke-dashoffset: 0; }\n  90%  { stroke-dashoffset: 0; }\n  100% { stroke-dashoffset: 48; }\n}\n\n/* ── Speed helpers ─────────────────────────────────────────────────── */\n\n.uicon-slow  { --uicon-duration: 3.5s; }\n.uicon-fast  { --uicon-duration: 1.1s; }\n\n/* ── Accessibility ─────────────────────────────────────────────────── */\n\n@media (prefers-reduced-motion: reduce) {\n  .uicon * {\n    animation: none !important;\n    stroke-dashoffset: 0 !important;\n  }\n}\n\n/* Detail sheet entrance: rises from the bottom on phones, fades in centred\n   from the sm breakpoint up. */\n@keyframes uiconSheetIn {\n  from { transform: translateY(100%); opacity: 0; }\n  to   { transform: translateY(0); opacity: 1; }\n}\n\n@keyframes uiconSheetInDesktop {\n  from { transform: scale(0.96); opacity: 0; }\n  to   { transform: scale(1); opacity: 1; }\n}\n\n.animate-sheet-in {\n  animation: uiconSheetIn 0.34s cubic-bezier(0.16, 1, 0.3, 1);\n}\n\n@media (min-width: 640px) {\n  .animate-sheet-in {\n    animation: uiconSheetInDesktop 0.24s cubic-bezier(0.16, 1, 0.3, 1);\n  }\n}\n";

  /* ── Stylesheet injection (once) ─────────────────────────────── */
  function injectCss() {
    if (typeof document === "undefined") return;
    if (document.getElementById("uflow-icons-css")) return;
    var tag = document.createElement("style");
    tag.id = "uflow-icons-css";
    tag.textContent = CSS;
    document.head.appendChild(tag);
  }
  injectCss();

  /* ── Style resolution — mirrors lib/uflow-icons/registry.ts ──── */
  function isFillable(part) {
    var t = part.t;
    if (t === "circle" || t === "rect" || t === "ellipse") return true;
    if (t === "path") return /z\s*$/i.test(String(part.a.d || "").trim());
    return false;
  }

  function resolvePaint(part, style, paint, index) {
    var fillable = isFillable(part);
    var primary = index === 0;

    if (part.f && style !== "stroke" && style !== "twotone") {
      return { fill: paint, stroke: "none" };
    }

    if (style === "solid") {
      return fillable
        ? { fill: paint, stroke: paint, strokeOpacity: 0 }
        : { fill: "none", stroke: paint };
    }
    if (style === "bulk") {
      return fillable
        ? { fill: paint, stroke: paint, fillOpacity: 0.22 }
        : { fill: "none", stroke: paint };
    }
    if (style === "duotone") {
      return fillable && primary
        ? { fill: paint, stroke: paint, fillOpacity: 0.3 }
        : { fill: "none", stroke: paint, strokeOpacity: primary ? 1 : 0.45 };
    }
    if (style === "twotone") {
      return {
        fill: part.f ? paint : "none",
        stroke: paint,
        strokeOpacity: primary ? 1 : 0.4,
        fillOpacity: part.f ? 0.45 : undefined
      };
    }
    return { fill: part.f ? paint : "none", stroke: part.f ? "none" : paint };
  }

  /* ── Shared attribute builder (DOM + React share this) ───────── */
  function buildSpec(opts) {
    var name = opts.name;
    var parts = ICONS[name];
    if (!parts) return null;

    var size = opts.size || 24;
    var style = opts.style || "bulk";
    var corner = opts.corner || "rounded";
    var gradient = opts.gradient || "uflow";
    var colors = opts.colors || null;
    var animated = !!opts.animated;
    var onHover = opts.animated === "hover" || opts.onHover === true;
    var sw = opts.strokeWidth || 1.75;

    var stops = colors || GRADIENTS[gradient] || GRADIENTS.uflow;
    var mono = !colors && gradient === "mono";
    var key = (colors ? colors.join("") : gradient).replace(/[^a-z0-9]/gi, "");
    var gid = "uicon-" + name + "-" + key;
    var paint = mono ? "currentColor" : "url(#" + gid + ")";
    var caps = CORNERS[corner] || CORNERS.rounded;

    return {
      parts: parts, size: size, style: style, sw: sw, caps: caps,
      mono: mono, stops: stops, gid: gid, paint: paint,
      cls: "uicon" + (animated ? (onHover ? " uicon-on-hover" : " uicon-animated") : ""),
      resolve: function (part, i) { return resolvePaint(part, style, paint, i); }
    };
  }

  /* ── DOM renderer ────────────────────────────────────────────── */
  function el(tag, attrs) {
    var node = document.createElementNS(NS, tag);
    for (var k in attrs) if (attrs[k] != null) node.setAttribute(k, attrs[k]);
    return node;
  }

  function render(opts) {
    var s = buildSpec(opts);
    if (!s) return null;

    var svg = el("svg", {
      viewBox: "0 0 24 24", width: s.size, height: s.size,
      fill: "none", stroke: s.paint, "stroke-width": s.sw,
      "stroke-linecap": s.caps.cap, "stroke-linejoin": s.caps.join,
      "stroke-miterlimit": s.caps.miter,
      "class": s.cls + (opts.className ? " " + opts.className : "")
    });

    if (opts.title) {
      svg.setAttribute("role", "img");
      svg.setAttribute("aria-label", opts.title);
    } else {
      svg.setAttribute("aria-hidden", "true");
    }

    if (!s.mono) {
      var defs = el("defs", {});
      var grad = el("linearGradient", {
        id: s.gid, x1: 0, y1: 0, x2: 24, y2: 24, gradientUnits: "userSpaceOnUse"
      });
      if (s.stops[2]) {
        grad.appendChild(el("stop", { offset: "0%", "stop-color": s.stops[0] }));
        grad.appendChild(el("stop", { offset: "50%", "stop-color": s.stops[1] }));
        grad.appendChild(el("stop", { offset: "100%", "stop-color": s.stops[2] }));
      } else {
        grad.appendChild(el("stop", { offset: "0%", "stop-color": s.stops[0] }));
        grad.appendChild(el("stop", { offset: "100%", "stop-color": s.stops[1] }));
      }
      defs.appendChild(grad);
      svg.appendChild(defs);
    }

    s.parts.forEach(function (p, i) {
      var node = el(p.t, p.a);
      if (p.m) node.setAttribute("class", p.m);
      var css = "";
      if (p.d) css += "animation-delay:" + p.d + "ms;";
      if (p.o) css += "transform-origin:" + p.o + ";transform-box:view-box;";
      if (css) node.setAttribute("style", css);

      var paint = s.resolve(p, i);
      node.setAttribute("fill", paint.fill);
      node.setAttribute("stroke", paint.stroke);
      if (paint.fillOpacity != null) node.setAttribute("fill-opacity", paint.fillOpacity);
      if (paint.strokeOpacity != null) node.setAttribute("stroke-opacity", paint.strokeOpacity);
      svg.appendChild(node);
    });

    return svg;
  }

  /* ── <u-icon> custom element ─────────────────────────────────── */
  if (typeof HTMLElement !== "undefined" && typeof customElements !== "undefined") {
    var UIconElement = function () { return Reflect.construct(HTMLElement, [], UIconElement); };
    UIconElement.prototype = Object.create(HTMLElement.prototype);
    UIconElement.prototype.constructor = UIconElement;
    Object.setPrototypeOf(UIconElement, HTMLElement);

    UIconElement.observedAttributes = [
      "name", "size", "style-name", "corner", "gradient",
      "colors", "animated", "stroke-width", "label"
    ];

    UIconElement.prototype.attributeChangedCallback = function () { this.draw(); };
    UIconElement.prototype.connectedCallback = function () { this.draw(); };

    UIconElement.prototype.draw = function () {
      var colors = this.getAttribute("colors");
      var svg = render({
        name: this.getAttribute("name"),
        size: this.getAttribute("size"),
        // "style" is reserved on HTMLElement, so the attribute is style-name.
        style: this.getAttribute("style-name") || this.getAttribute("variant"),
        corner: this.getAttribute("corner"),
        gradient: this.getAttribute("gradient"),
        colors: colors ? colors.split(",").map(function (c) { return c.trim(); }) : null,
        animated: this.hasAttribute("animated") ? (this.getAttribute("animated") || true) : false,
        strokeWidth: this.getAttribute("stroke-width"),
        title: this.getAttribute("label")
      });
      this.textContent = "";
      if (svg) this.appendChild(svg);
    };

    if (!customElements.get("u-icon")) customElements.define("u-icon", UIconElement);
  }

  /* ── React component (optional, only if React is present) ────── */
  function createIcon(React) {
    return function Icon(props) {
      var s = buildSpec(props);
      if (!s) return null;
      var h = React.createElement;

      var children = [];

      if (!s.mono) {
        var stopEls = s.stops[2]
          ? [
              h("stop", { key: 0, offset: "0%", stopColor: s.stops[0] }),
              h("stop", { key: 1, offset: "50%", stopColor: s.stops[1] }),
              h("stop", { key: 2, offset: "100%", stopColor: s.stops[2] })
            ]
          : [
              h("stop", { key: 0, offset: "0%", stopColor: s.stops[0] }),
              h("stop", { key: 1, offset: "100%", stopColor: s.stops[1] })
            ];
        children.push(
          h("defs", { key: "d" },
            h("linearGradient", {
              id: s.gid, x1: 0, y1: 0, x2: 24, y2: 24, gradientUnits: "userSpaceOnUse"
            }, stopEls))
        );
      }

      s.parts.forEach(function (p, i) {
        var paint = s.resolve(p, i);
        var css = {};
        if (p.d) css.animationDelay = p.d + "ms";
        if (p.o) { css.transformOrigin = p.o; css.transformBox = "view-box"; }

        var attrs = { key: i, className: p.m };
        for (var k in p.a) attrs[k] = p.a[k];
        attrs.fill = paint.fill;
        attrs.stroke = paint.stroke;
        if (paint.fillOpacity != null) attrs.fillOpacity = paint.fillOpacity;
        if (paint.strokeOpacity != null) attrs.strokeOpacity = paint.strokeOpacity;
        if (Object.keys(css).length) attrs.style = css;

        children.push(h(p.t, attrs));
      });

      return h("svg", {
        xmlns: NS, viewBox: "0 0 24 24",
        width: s.size, height: s.size,
        fill: "none", stroke: s.paint, strokeWidth: s.sw,
        strokeLinecap: s.caps.cap, strokeLinejoin: s.caps.join,
        strokeMiterlimit: s.caps.miter,
        className: s.cls + (props.className ? " " + props.className : ""),
        role: props.title ? "img" : undefined,
        "aria-label": props.title,
        "aria-hidden": props.title ? undefined : true
      }, children);
    };
  }

  var api = {
    version: VERSION,
    names: Object.keys(ICONS),
    gradients: Object.keys(GRADIENTS),
    styles: ["bulk","stroke","solid","duotone","twotone"],
    corners: ["rounded","standard","sharp"],
    icons: ICONS,
    render: render,
    createIcon: createIcon
  };

  // Auto-wire the React component when React is already on the page.
  if (typeof window !== "undefined" && window.React) {
    api.Icon = createIcon(window.React);
  }

  return api;
});
