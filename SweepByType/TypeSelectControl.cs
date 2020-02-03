﻿/*
 * Copyright 2020 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using PeterHan.PLib;
using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.SweepByType {
	/// <summary>
	/// A control which allows selection of types.
	/// </summary>
	public sealed class TypeSelectControl {
		/// <summary>
		/// The margin around the scrollable area to avoid stomping on the scrollbar.
		/// </summary>
		private static readonly RectOffset ELEMENT_MARGIN = new RectOffset(2, 2, 2, 2);

		/// <summary>
		/// The indent of the categories, and the items in each category.
		/// </summary>
		internal const int INDENT = 24;

		/// <summary>
		/// The size of the panel (UI sizes are hard coded in prefabs).
		/// </summary>
		internal static readonly Vector2 PANEL_SIZE = new Vector2(260.0f, 320.0f);

		/// <summary>
		/// The margin between the scroll pane and the window.
		/// </summary>
		private static readonly RectOffset OUTER_MARGIN = new RectOffset(6, 10, 6, 14);

		/// <summary>
		/// The size of checkboxes and images in this control.
		/// </summary>
		internal static readonly Vector2 ROW_SIZE = new Vector2(24.0f, 24.0f);

		/// <summary>
		/// The spacing between each row.
		/// </summary>
		internal const int ROW_SPACING = 2;

		/// <summary>
		/// Gets the sprite for a particular element tag.
		/// </summary>
		/// <param name="elementTag">The tag of the element to look up.</param>
		/// <param name="tint">The tint which will be used for the image.</param>
		/// <returns>The sprite to use for it.</returns>
		internal static Sprite GetStorageObjectSprite(Tag elementTag, out Color tint) {
			Sprite result = null;
			var prefab = Assets.GetPrefab(elementTag);
			tint = Color.white;
			if (prefab != null) {
				// Extract the UI preview image (sucks for bottles, but it is all we have)
				var sprite = Def.GetUISprite(prefab);
				if (sprite != null) {
					tint = sprite.second;
					result = sprite.first;
				}
			}
			return result;
		}

		/// <summary>
		/// Updates the all check box state from the children.
		/// </summary>
		/// <param name="allItems">The "all" or "none" check box.</param>
		/// <param name="children">The child check boxes.</param>
		internal static void UpdateAllItems<T>(GameObject allItems,
				IEnumerable<T> children) where T : IHasCheckBox {
			if (allItems != null) {
				bool all = true, none = true;
				foreach (var child in children)
					if (PCheckBox.GetCheckState(child.CheckBox) == PCheckBox.STATE_CHECKED)
						none = false;
					else
						// Partially checked or unchecked
						all = false;
				PCheckBox.SetCheckState(allItems, none ? PCheckBox.STATE_UNCHECKED : (all ?
					PCheckBox.STATE_CHECKED : PCheckBox.STATE_PARTIAL));
			}
		}

		/// <summary>
		/// Returns true if all items are selected to sweep.
		/// </summary>
		public bool IsAllSelected {
			get {
				return PCheckBox.GetCheckState(allItems) == PCheckBox.STATE_CHECKED;
			}
		}

		/// <summary>
		/// Whether material icons should be disabled.
		/// </summary>
		public bool DisableIcons { get; }

		/// <summary>
		/// The root panel of the whole control.
		/// </summary>
		public GameObject RootPanel { get; }

		/// <summary>
		/// The screen object.
		/// </summary>
		public KScreen Screen { get; }

		/// <summary>
		/// The "all items" checkbox.
		/// </summary>
		private GameObject allItems;

		/// <summary>
		/// The child panel where all categories are added.
		/// </summary>
		private GameObject childPanel;

		/// <summary>
		/// The child categories.
		/// </summary>
		private readonly SortedList<Tag, TypeSelectCategory> children;

		public TypeSelectControl(bool disableIcons = false) {
			DisableIcons = disableIcons;
			// Select/deselect all types
			var allCheckBox = new PCheckBox("SelectAll") {
				Text = STRINGS.UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON,
				CheckSize = ROW_SIZE, InitialState = PCheckBox.STATE_CHECKED,
				OnChecked = OnCheck, TextStyle = PUITuning.Fonts.TextDarkStyle
			};
			allCheckBox.OnRealize += (obj) => { allItems = obj; };
			var cp = new PPanel("Categories") {
				Direction = PanelDirection.Vertical, Alignment = TextAnchor.UpperLeft,
				Spacing = ROW_SPACING
			};
			cp.OnRealize += (obj) => { childPanel = obj; };
			RootPanel = new PPanel("Border") {
				// 1px dark border for contrast
				Margin = new RectOffset(1, 1, 1, 1), Direction = PanelDirection.Vertical,
				Alignment = TextAnchor.MiddleCenter, Spacing = 1
			}.AddChild(new PLabel("Title") {
				// Title bar
				TextAlignment = TextAnchor.MiddleCenter, Text = SweepByTypeStrings.
				DIALOG_TITLE, FlexSize = new Vector2(1.0f, 0.0f), DynamicSize = true,
				Margin = new RectOffset(1, 1, 1, 1)
			}.SetKleiPinkColor()).AddChild(new PPanel("TypeSelectControl") {
				// White background for scroll bar
				Direction = PanelDirection.Vertical, Margin = OUTER_MARGIN,
				Alignment = TextAnchor.MiddleCenter, Spacing = 0,
				BackColor = PUITuning.Colors.BackgroundLight, FlexSize = Vector2.one
			}.AddChild(new PScrollPane("Scroll") {
				// Scroll to select elements
				Child = new PPanel("SelectType") {
					Direction = PanelDirection.Vertical, Margin = ELEMENT_MARGIN,
					FlexSize = new Vector2(1.0f, 0.0f), Alignment = TextAnchor.UpperLeft
				}.AddChild(allCheckBox).AddChild(cp), ScrollHorizontal = false,
				ScrollVertical = true, AlwaysShowVertical = true, TrackSize = 8.0f,
				FlexSize = Vector2.one, BackColor = PUITuning.Colors.BackgroundLight,
			})).SetKleiBlueColor().BuildWithFixedSize(PANEL_SIZE);
			children = new SortedList<Tag, TypeSelectCategory>(16, TagAlphabetComparer.
				INSTANCE);
			Screen = RootPanel.AddComponent<TypeSelectScreen>();
		}

		/// <summary>
		/// Adds selected types in this category to the list of items to sweep.
		/// </summary>
		/// <param name="items">The location where selected types will be stored.</param>
		public void AddTypesToSweep(ICollection<Tag> items) {
			foreach (var child in children)
				child.Value.AddTypesToSweep(items);
		}

		/// <summary>
		/// Selects all items.
		/// </summary>
		public void CheckAll() {
			PCheckBox.SetCheckState(allItems, PCheckBox.STATE_CHECKED);
			foreach (var child in children)
				child.Value.CheckAll();
		}

		/// <summary>
		/// Deselects all items.
		/// </summary>
		public void ClearAll() {
			PCheckBox.SetCheckState(allItems, PCheckBox.STATE_UNCHECKED);
			foreach (var child in children)
				child.Value.ClearAll();
		}

		private void OnCheck(GameObject source, int state) {
			if (state == PCheckBox.STATE_UNCHECKED)
				// Clicked when unchecked, check all
				CheckAll();
			else
				// Clicked when checked or partial, clear all
				ClearAll();
		}

		/// <summary>
		/// Updates the list of available elements.
		/// </summary>
		public void Update() {
			var inventory = WorldInventory.Instance;
			if (inventory != null) {
				// Find categories with discovered materials
				// This is the same logic as used in ResourceCategoryScreen
				foreach (var category in GameTags.MaterialCategories)
					UpdateCategory(inventory, category);
				foreach (var category in GameTags.CalorieCategories)
					UpdateCategory(inventory, category);
				foreach (var category in GameTags.UnitCategories)
					UpdateCategory(inventory, category);
				UpdateCategory(inventory, GameTags.Miscellaneous);
				UpdateCategory(inventory, GameTags.MiscPickupable, SweepByTypeStrings.
					CATEGORY_MISCPICKUPABLE);
			}
		}

		/// <summary>
		/// Updates all elements in the specified category.
		/// </summary>
		/// <param name="inv">The inventory of discovered elements.</param>
		/// <param name="category">The category to search.</param>
		/// <param name="overrideName">The name to override the category title</param>
		private void UpdateCategory(WorldInventory inv, Tag category,
				string overrideName = null) {
			if (inv.TryGetDiscoveredResourcesFromTag(category, out HashSet<Tag> found) &&
					found.Count > 0) {
				// Attempt to add to type select control
				if (!children.TryGetValue(category, out TypeSelectCategory current)) {
					current = new TypeSelectCategory(this, category, overrideName);
					children.Add(category, current);
					int index = children.IndexOfKey(category) << 1;
					GameObject header = current.Header, panel = current.ChildPanel;
					// Header goes in even indexes, panel goes in odds
					header.SetParent(childPanel);
					PUIElements.SetAnchors(header, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
					header.transform.SetSiblingIndex(index);
					panel.SetParent(childPanel);
					PUIElements.SetAnchors(panel, PUIAnchoring.Stretch, PUIAnchoring.Stretch);
					panel.transform.SetSiblingIndex(index + 1);
				}
				foreach (var element in found)
					current.TryAddType(element);
			}
		}

		/// <summary>
		/// Updates the parent check box state from the children.
		/// </summary>
		internal void UpdateFromChildren() {
			UpdateAllItems(allItems, children.Values);
		}

		/// <summary>
		/// A category used in type select controls.
		/// </summary>
		private sealed class TypeSelectCategory : IHasCheckBox {
			/// <summary>
			/// The tag for this category.
			/// </summary>
			public Tag CategoryTag { get; }

			/// <summary>
			/// The check box for selecting or deselecting children.
			/// </summary>
			public GameObject CheckBox { get; private set; }

			/// <summary>
			/// The panel holding all children.
			/// </summary>
			public GameObject ChildPanel { get; }

			/// <summary>
			/// The parent control.
			/// </summary>
			public TypeSelectControl Control { get; }

			/// <summary>
			/// The header for this category.
			/// </summary>
			public GameObject Header { get; }

			/// <summary>
			/// The child elements.
			/// </summary>
			private readonly SortedList<Tag, TypeSelectElement> children;

			internal TypeSelectCategory(TypeSelectControl parent, Tag categoryTag,
					string overrideName = null) {
				Control = parent ?? throw new ArgumentNullException("parent");
				CategoryTag = categoryTag;
				string title = string.IsNullOrEmpty(overrideName) ? CategoryTag.ProperName() :
					overrideName;
				var selectBox = new PCheckBox("SelectCategory") {
					Text = title, DynamicSize = true, OnChecked = OnCheck,
					CheckSize = ROW_SIZE, InitialState = PCheckBox.STATE_CHECKED,
					TextStyle = PUITuning.Fonts.TextDarkStyle
				};
				selectBox.OnRealize += (obj) => { CheckBox = obj; };
				Header = new PPanel("TypeSelectCategory") {
					Direction = PanelDirection.Horizontal, Alignment = TextAnchor.MiddleLeft,
					Spacing = 5
				}.AddChild(new PToggle("ShowHide") {
					OnStateChanged = OnToggle, Size = new Vector2(ROW_SIZE.x * 0.5f,
					ROW_SIZE.y * 0.5f), Color = PUITuning.Colors.ComponentLightStyle
				}).AddChild(selectBox).Build();
				children = new SortedList<Tag, TypeSelectElement>(16, TagAlphabetComparer.
					INSTANCE);
				ChildPanel = new PPanel("Children") {
					Direction = PanelDirection.Vertical, Alignment = TextAnchor.UpperLeft,
					Spacing = ROW_SPACING, Margin = new RectOffset(INDENT, 0, 0, 0)
				}.Build();
				ChildPanel.transform.localScale = Vector3.zero;
			}

			/// <summary>
			/// Adds selected types in this category to the list of items to sweep.
			/// </summary>
			/// <param name="items">The location where selected types will be stored.</param>
			internal void AddTypesToSweep(ICollection<Tag> items) {
				foreach (var child in children) {
					var element = child.Value;
					if (PCheckBox.GetCheckState(element.CheckBox) == PCheckBox.STATE_CHECKED)
						items.Add(child.Key);
				}
			}

			/// <summary>
			/// Selects all items in this category.
			/// </summary>
			public void CheckAll() {
				PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_CHECKED);
				foreach (var child in children)
					PCheckBox.SetCheckState(child.Value.CheckBox, PCheckBox.STATE_CHECKED);
			}

			/// <summary>
			/// Deselects all items in this category.
			/// </summary>
			public void ClearAll() {
				PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_UNCHECKED);
				foreach (var child in children)
					PCheckBox.SetCheckState(child.Value.CheckBox, PCheckBox.STATE_UNCHECKED);
			}

			private void OnCheck(GameObject source, int state) {
				if (state == PCheckBox.STATE_UNCHECKED)
					// Clicked when unchecked, check all
					CheckAll();
				else
					// Clicked when checked or partial, clear all
					ClearAll();
				Control.UpdateFromChildren();
			}

			private void OnToggle(GameObject source, bool open) {
				var obj = ChildPanel;
				if (obj != null)
					// Scale to 0x0 if not visible
					obj.rectTransform().localScale = open ? Vector3.one : Vector3.zero;
			}

			/// <summary>
			/// Attempts to add a type to this category.
			/// </summary>
			/// <param name="element">The type to add.</param>
			/// <returns>true if it was added, or false if it already exists.</returns>
			public bool TryAddType(Tag element) {
				bool add = !children.ContainsKey(element);
				if (add) {
					var child = new TypeSelectElement(this, element);
					var cb = child.CheckBox;
					// Add the element to the list, then get its index and move it in the panel
					// to maintain sorted order
					children.Add(element, child);
					cb.SetParent(ChildPanel);
					if (PCheckBox.GetCheckState(cb) == PCheckBox.STATE_CHECKED)
						// Set to checked
						PCheckBox.SetCheckState(cb, PCheckBox.STATE_CHECKED);
					cb.transform.SetSiblingIndex(children.IndexOfKey(element));
				}
				return add;
			}

			/// <summary>
			/// Updates the parent check box state from the children.
			/// </summary>
			internal void UpdateFromChildren() {
				UpdateAllItems(CheckBox, children.Values);
				Control.UpdateFromChildren();
			}
		}

		/// <summary>
		/// An individual element choice used in type select controls.
		/// </summary>
		private sealed class TypeSelectElement : IHasCheckBox {
			/// <summary>
			/// The selection checkbox.
			/// </summary>
			public GameObject CheckBox { get; }

			/// <summary>
			/// The tag for this element.
			/// </summary>
			public Tag ElementTag { get; }

			/// <summary>
			/// The parent category.
			/// </summary>
			private readonly TypeSelectCategory parent;

			internal TypeSelectElement(TypeSelectCategory parent, Tag elementTag) {
				this.parent = parent ?? throw new ArgumentNullException("parent");
				var tint = Color.white;
				var sprite = parent.Control.DisableIcons ? null :
					GetStorageObjectSprite(elementTag, out tint);
				ElementTag = elementTag;
				CheckBox = new PCheckBox("Select") {
					CheckSize = ROW_SIZE, SpriteSize = ROW_SIZE, OnChecked = OnCheck,
					Text = ElementTag.ProperName(), InitialState = PCheckBox.
					STATE_CHECKED, Sprite = sprite, SpriteTint = tint,
					TextStyle = PUITuning.Fonts.TextDarkStyle
				}.Build();
			}

			private void OnCheck(GameObject source, int state) {
				if (state == PCheckBox.STATE_UNCHECKED)
					// Clicked when unchecked, check and possibly check all
					PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_CHECKED);
				else
					// Clicked when checked, clear and possibly uncheck
					PCheckBox.SetCheckState(CheckBox, PCheckBox.STATE_UNCHECKED);
				parent.UpdateFromChildren();
			}

			public override string ToString() {
				return "TypeSelectElement[Tag={0},State={1}]".F(ElementTag.ToString(),
					PCheckBox.GetCheckState(CheckBox));
			}
		}

		/// <summary>
		/// Applied to categories and elements with a single summary checkbox.
		/// </summary>
		internal interface IHasCheckBox {
			/// <summary>
			/// Checkbox!
			/// </summary>
			GameObject CheckBox { get; }
		}

		/// <summary>
		/// The screen type used for a type select control.
		/// </summary>
		private sealed class TypeSelectScreen : KScreen {
			public TypeSelectScreen() {
				activateOnSpawn = true;
				ConsumeMouseScroll = true;
			}
		}
	}
}
