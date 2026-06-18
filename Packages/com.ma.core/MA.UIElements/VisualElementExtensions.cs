// Copyright © Magnetic Arcade. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MA.Core
{
    public static class VisualElementExtensions
    {
        /// <summary>Shows the element.</summary>
        /// <param name="element">The element to show.</param>
        public static void Show(this VisualElement element) 
            => element.style.display = DisplayStyle.Flex;

        /// <summary>Hides the element.</summary>
        /// <param name="element">The element to hide.</param>
        public static void Hide(this VisualElement element) 
            => element.style.display = DisplayStyle.None;

        /// <summary>Returns all children of the element that are of type <typeparamref name="T"/>.</summary>
        /// <param name="element">The element to search from.</param>
        public static IEnumerable<T> ChildrenOfType<T>(this VisualElement element)
        {
            if (element != null)
            {
                foreach (VisualElement child in element.Children())
                {
                    if (child is T t)
                    {
                        yield return t;
                    }

                    foreach (T e in child.ChildrenOfType<T>())
                    {
                        yield return e;
                    }
                }
            }
        }   
        
        /// <summary>Returns the first ancestor of the element that has the class <paramref name="className"/>, or null if none is found.</summary>
        /// <param name="element">The element to search from.</param>
        /// <param name="className">The class name to search for.</param>
        public static VisualElement GetFirstAncestorWithClass(this VisualElement element, string className)
        {
            if (element == null)
                return null;
     
            if (element.ClassListContains(className))
                return element;
     
            return element.parent.GetFirstAncestorWithClass(className);
        }


        /// <summary>Returns all children of the element that are of type <typeparamref name="T"/>.</summary>
        /// <param name="element">The element to search from.</param>
        public static T FirstChildOfType<T>(this VisualElement element) 
            where T : VisualElement
        {
            if (element == null)
                return null;
            
            foreach (VisualElement child in element.Children())
            {
                if (child is T t)
                {
                    return t;
                }

                foreach (T e in child.ChildrenOfType<T>())
                {
                    return e;
                }
            }

            return null;
        }
    }
}
