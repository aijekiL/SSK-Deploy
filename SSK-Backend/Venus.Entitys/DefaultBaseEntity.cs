namespace Venus.Entitys
{
    public class DefaultBaseEntity
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        public Guid CreateID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 修改用户
        /// </summary>
        public Guid? ModifyID { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 删除用户ID
        /// </summary>
        public Guid? DeleteId { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
