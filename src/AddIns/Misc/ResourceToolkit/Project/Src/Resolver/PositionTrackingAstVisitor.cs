// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Christian Hornung" email=""/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;

namespace Hornung.ResourceToolkit.Resolver
{
	/// <summary>
	/// Provides contextual position information while iterating through
	/// the AST and the ability to resolve expressions in-place.
	/// </summary>
	public abstract class PositionTrackingAstVisitor : ICSharpCode.NRefactory.Visitors.NodeTrackingAstVisitor
	{
		
		private Stack<INode> parentNodes;
		
		protected override void BeginVisit(INode node)
		{
			base.BeginVisit(node);
			// Only push nodes on the stack which have valid position information.
			if (node != null &&
			    node.StartLocation.X >= 1 && node.StartLocation.Y >= 1 &&
			    node.EndLocation.X >= 1 && node.EndLocation.Y >= 1) {
				this.parentNodes.Push(node);
			}
		}
		
		protected override void EndVisit(INode node)
		{
			base.EndVisit(node);
			// Only remove those nodes which have actually been pushed before.
			if (this.parentNodes.Count > 0 && INode.ReferenceEquals(this.parentNodes.Peek(), node)) {
				this.parentNodes.Pop();
			}
		}
		
		// ********************************************************************************************************************************
		
		/// <summary>
		/// Gets a flag that indicates whether the current node is located
		/// inside a block which position information is available for.
		/// </summary>
		protected bool PositionAvailable {
			get {
				return this.parentNodes.Count > 0;
			}
		}
		
		/// <summary>
		/// Gets the start location of the current innermost node with valid position information
		/// as 1-based coordinates in the parsed document.
		/// X = column number, Y = line number.
		/// </summary>
		protected Location CurrentNodeStartLocation {
			get {
				return this.parentNodes.Peek().StartLocation;
			}
		}
		
		/// <summary>
		/// Gets the end location of the current innermost node with valid position information
		/// as 1-based coordinates in the parsed document.
		/// X = column number, Y = line number.
		/// </summary>
		protected Location CurrentNodeEndLocation {
			get {
				return this.parentNodes.Peek().EndLocation;
			}
		}
		
		// ********************************************************************************************************************************
		
		private CompilationUnit compilationUnit;
		
		public override object TrackedVisitCompilationUnit(CompilationUnit compilationUnit, object data)
		{
			this.compilationUnit = compilationUnit;
			return base.TrackedVisitCompilationUnit(compilationUnit, data);
		}
		
		// ********************************************************************************************************************************
		
		/// <summary>
		/// Resolves an expression in the current node's context.
		/// </summary>
		/// <param name="expression">The expression to be resolved.</param>
		/// <param name="fileName">The file name of the source file that contains the expression to be resolved.</param>
		public ResolveResult Resolve(Expression expression, string fileName)
		{
			return this.Resolve(expression, fileName, ExpressionContext.Default);
		}
		
		/// <summary>
		/// Resolves an expression in the current node's context.
		/// </summary>
		/// <param name="expression">The expression to be resolved.</param>
		/// <param name="fileName">The file name of the source file that contains the expression to be resolved.</param>
		/// <param name="context">The ExpressionContext.</param>
		public ResolveResult Resolve(Expression expression, string fileName, ExpressionContext context)
		{
			if (!this.PositionAvailable) {
				LoggingService.Info("ResourceToolkit: PositionTrackingAstVisitor: Resolve failed due to position information being unavailable. Expression: "+expression.ToString());
				return null;
			}
			
			#if DEBUG
			LoggingService.Debug("ResourceToolkit: PositionTrackingAstVisitor: Using this parent node for resolve: "+this.parentNodes.Peek().ToString());
			#endif
			
			return NRefactoryAstCacheService.ResolveLowLevel(fileName, this.CurrentNodeStartLocation.Y, this.CurrentNodeStartLocation.X+1, this.compilationUnit, null, expression, context);
		}
		
		// ********************************************************************************************************************************
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PositionTrackingAstVisitor"/> class.
		/// </summary>
		protected PositionTrackingAstVisitor() : base()
		{
			this.parentNodes = new Stack<INode>();
		}
	}
}
