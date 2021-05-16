using System;
using Azure;
using Azure.Data.Tables;

namespace wotbot.Domain
{
    public abstract class BaseTableEntity : ITableEntity, IEquatable<BaseTableEntity> {
    
        private readonly DateTimeOffset? _timestamp = default!;
        private ETag _eTag = default!;
        public abstract string PartitionKey { get; set; }
        public abstract string RowKey{ get; set; }
        DateTimeOffset? ITableEntity.Timestamp => _timestamp;
        ETag ITableEntity.ETag
        {
            get => _eTag;
            set => _eTag = value;
        }

        public bool Equals(BaseTableEntity? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PartitionKey == other.PartitionKey && RowKey == other.RowKey;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseTableEntity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PartitionKey.GetHashCode() * 397) ^ RowKey.GetHashCode();
            }
        }

        public static bool operator ==(BaseTableEntity? left, BaseTableEntity? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseTableEntity? left, BaseTableEntity? right)
        {
            return !Equals(left, right);
        }
    }
}