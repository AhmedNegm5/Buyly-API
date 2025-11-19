using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Buyly.Domain.Specifications
{
    public class BaseSpecification<T> : ISpecification<T>
    {
        public List<Expression<Func<T, bool>>> Criteria { get; } = new();

        public List<Expression<Func<T, object>>> Includes { get; } = new();

        public Expression<Func<T, object>>? OrderBy { get; private set; }

        public Expression<Func<T, object>>? OrderByDescending { get; private set; }

        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPagingEnabled { get; private set; }
        public bool IsTrackingEnabled { get; private set; } = true;

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        protected void AddCriteria(Expression<Func<T, bool>> criteria)
        {
            Criteria.Add(criteria);
        }

        protected void AsNoTracking()
        {
            IsTrackingEnabled = false;
        }
    }
}