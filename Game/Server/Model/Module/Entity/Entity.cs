using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [Flags]
    public enum EntityStatus: byte
    {
        None = 0,
        IsFromPool = 1,
        IsRegister = 1 << 1,
        IsComponent = 1 << 2,
        IsCreated = 1 << 3,
        IsNew = 1 << 4,
    }

    [SuppressMessage("ETHotfixProjectAnalyzers", "ET0014:禁止在Entity类中直接调用Child和Component")]
    public partial class Entity: DisposeObject
    {
        [BsonIgnore]
        public long InstanceId
        {
            get;
            protected set;
        }

        protected Entity()
        {
        }

        [BsonIgnore]
        private EntityStatus status = EntityStatus.None;

        [BsonIgnore]
        private bool IsFromPool
        {
            get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this.status &= ~EntityStatus.IsFromPool;
                }
            }
        }

        [BsonIgnore]
        protected bool IsRegister
        {
            get => (this.status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
            set
            {
                if (this.IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    this.status |= EntityStatus.IsRegister;
                }
                else
                {
                    this.status &= ~EntityStatus.IsRegister;
                }
				
				
                if (!value)
                {
                    Root.Instance.Remove(this.InstanceId);
                }
                else
                {
                    Root.Instance.Add(this);
                    EventSystem.Instance.RegisterSystem(this);
                }
            }
        }
        

        [BsonIgnore]
        private bool IsComponent
        {
            get => (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsComponent;
                }
                else
                {
                    this.status &= ~EntityStatus.IsComponent;
                }
            }
        }

        [BsonIgnore]
        protected bool IsCreated
        {
            get => (this.status & EntityStatus.IsCreated) == EntityStatus.IsCreated;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsCreated;
                }
                else
                {
                    this.status &= ~EntityStatus.IsCreated;
                }
            }
        }
        
        [BsonIgnore]
        protected bool IsNew
        {
            get => (this.status & EntityStatus.IsNew) == EntityStatus.IsNew;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsNew;
                }
                else
                {
                    this.status &= ~EntityStatus.IsNew;
                }
            }
        }

        [BsonIgnore]
        public bool IsDisposed => this.InstanceId == 0;

        [BsonIgnore]
        protected Entity parent;

        // 可以改变parent，但是不能设置为null
        [BsonIgnore]
        public Entity Parent
        {
            get => this.parent;
            private set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().Name}");
                }
                
                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().Name}");
                }

                // 严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.Domain == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {this.GetType().Name} {value.GetType().Name}");
                }

                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent == value)
                    {
                        Log.GetLogger().Error($"重复设置了Parent: {this.GetType().Name} parent: {this.parent.GetType().Name}");
                        return;
                    }
                    this.parent.RemoveFromChildren(this);
                }
                
                this.parent = value;
                this.IsComponent = false;
                this.parent.AddToChildren(this);
                this.Domain = this.parent.domain;
                
            }
        }

        // 该方法只能在AddComponent中调用，其他人不允许调用
        [BsonIgnore]
        private Entity ComponentParent
        {
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().Name}");
                }
                
                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().Name}");
                }
                
                // 严格限制parent必须要有domain,也就是说parent必须在数据树上面
                if (value.Domain == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {this.GetType().Name} {value.GetType().Name}");
                }
                
                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent == value)
                    {
                        Log.GetLogger().Error($"重复设置了Parent: {this.GetType().Name} parent: {this.parent.GetType().Name}");
                        return;
                    }
                    this.parent.RemoveFromComponents(this);
                }

                this.parent = value;
                this.IsComponent = true;
                this.parent.AddToComponents(this);
                this.Domain = this.parent.domain;
            }
        }

        public T GetParent<T>() where T : Entity
        {
            return this.Parent as T;
        }

        [BsonIgnoreIfDefault]
        [BsonElement]
        [BsonId]
        public long Id
        {
            get;
            set;
        }
        
        public int Version; // 存库数据的版本号 防止逻辑上的问题导致数据重复存库

        [BsonIgnore]
        protected Entity domain;

        [BsonIgnore]
        public Entity Domain
        {
            get
            {
                return this.domain;
            }
            private set
            {
                if (value == null)
                {
                    throw new Exception($"domain cant set null: {this.GetType().Name}");
                }
                
                if (this.domain == value)
                {
                    return;
                }
                
                Entity preDomain = this.domain;
                this.domain = value;
                
                if (preDomain == null)
                {
                    this.InstanceId = IdGenerater.Instance.GenerateInstanceId(Options.Instance.Process);
                    this.IsRegister = true;
                    
                    // 反序列化出来的需要设置父子关系
                    if (this.componentsDB != null)
                    {
                        foreach (Entity component in this.componentsDB)
                        {
                            component.IsComponent = true;
                            this.Components.Add(component.GetType(), component);
                            component.parent = this;
                        }
                    }

                    if (this.childrenDB != null)
                    {
                        foreach (Entity child in this.childrenDB)
                        {
                            child.IsComponent = false;
                            this.Children.Add(child.Id, child);
                            child.parent = this;
                        }
                    }
                }

                // 递归设置孩子的Domain
                if (this.children != null)
                {
                    foreach (Entity entity in this.children.Values)
                    {
                        entity.Domain = this.domain;
                    }
                }

                if (this.components != null)
                {
                    foreach (Entity component in this.components.Values)
                    {
                        component.Domain = this.domain;
                    }
                }

                if (!this.IsCreated)
                {
                    this.IsCreated = true;
                    EventSystem.Instance.Deserialize(this);
                }
            }
        }

        [BsonElement("Children")]
        [BsonIgnoreIfNull]
        private HashSet<Entity> childrenDB;

        [BsonIgnore]
        private Dictionary<long, Entity> children;
        
        [BsonIgnore]
        public Dictionary<long, Entity> Children
        {
            get
            {
                return this.children ??= ObjectPool.Instance.Fetch<Dictionary<long, Entity>>();
            }
        }

        private void AddToChildren(Entity entity)
        {
            this.Children.Add(entity.Id, entity);
            this.AddToChildrenDB(entity);
        }

        private void RemoveFromChildren(Entity entity)
        {
            if (this.children == null)
            {
                return;
            }

            this.children.Remove(entity.Id);
            if (this.children.Count == 0)
            {
                ObjectPool.Instance.Recycle(this.children);
                this.children = null;
            }

            this.RemoveFromChildrenDB(entity);
        }

        private void AddToChildrenDB(Entity entity)
        {
            if (!(entity is ISerializeToEntity))
            {
                return;
            }

            this.childrenDB ??= ObjectPool.Instance.Fetch<HashSet<Entity>>();

            this.childrenDB.Add(entity);
        }

        private void RemoveFromChildrenDB(Entity entity)
        {
            if (!(entity is ISerializeToEntity))
            {
                return;
            }

            if (this.childrenDB == null)
            {
                return;
            }

            this.childrenDB.Remove(entity);

            if (this.childrenDB.Count == 0 && this.IsNew)
            {
                ObjectPool.Instance.Recycle(this.childrenDB);
                this.childrenDB = null;
            }
        }

        [BsonElement("C")]
        [BsonIgnoreIfNull]
        private HashSet<Entity> componentsDB;

        [BsonIgnore]
        private Dictionary<Type, Entity> components;

        [BsonIgnore]
        public Dictionary<Type, Entity> Components
        {
            get
            {
                return this.components ??= ObjectPool.Instance.Fetch<Dictionary<Type, Entity>>();
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsRegister = false;
            this.InstanceId = 0;

            // 清理Children
            if (this.children != null)
            {
                foreach (Entity child in this.children.Values)
                {
                    child.Dispose();
                }

                this.children.Clear();
                ObjectPool.Instance.Recycle(this.children);
                this.children = null;

                if (this.childrenDB != null)
                {
                    this.childrenDB.Clear();
                    // 创建的才需要回到池中,从db中不需要回收
                    if (this.IsNew)
                    {
                        ObjectPool.Instance.Recycle(this.childrenDB);
                        this.childrenDB = null;
                    }
                }
            }
            
            // 清理Component
            if (this.components != null)
            {
                foreach (KeyValuePair<Type, Entity> kv in this.components)
                {
                    kv.Value.Dispose();
                }

                this.components.Clear();
                ObjectPool.Instance.Recycle(this.components);
                this.components = null;

                // 创建的才需要回到池中,从db中不需要回收
                if (this.componentsDB != null)
                {
                    this.componentsDB.Clear();
                    if (this.IsNew)
                    {
                        ObjectPool.Instance.Recycle(this.componentsDB);
                        this.componentsDB = null;
                    }
                }
            }

            // 触发Destroy事件
            if (this is IDestroy)
            {
                EventSystem.Instance.Destroy(this);
            }

            this.domain = null;

            if (this.parent != null && !this.parent.IsDisposed)
            {
                if (this.IsComponent)
                {
                    this.parent.RemoveComponent(this);
                }
                else
                {
                    this.parent.RemoveFromChildren(this);
                }
            }

            this.parent = null;

            base.Dispose();
            
            if (this.IsFromPool)
            {
                ObjectPool.Instance.Recycle(this);
            }
            status = EntityStatus.None;
        }

        private void AddToComponentsDB(Entity component)
        {
            if (!(component is ISerializeToEntity))
            {
                return;
            }
            
            this.componentsDB ??= ObjectPool.Instance.Fetch<HashSet<Entity>>();
            this.componentsDB.Add(component);
        }

        private void RemoveFromComponentsDB(Entity component)
        {
            if (!(component is ISerializeToEntity))
            {
                return;
            }
            
            if (this.componentsDB == null)
            {
                return;
            }

            this.componentsDB.Remove(component);
            if (this.componentsDB.Count == 0 && this.IsNew)
            {
                ObjectPool.Instance.Recycle(this.componentsDB);
                this.componentsDB = null;
            }
        }

        private void AddToComponents(Entity component)
        {
            this.Components.Add(component.GetType(), component);
            this.AddToComponentsDB(component);
        }

        private void RemoveFromComponents(Entity component)
        {
            if (this.components == null)
            {
                return;
            }

            this.components.Remove(component.GetType());

            if (this.components.Count == 0)
            {
                ObjectPool.Instance.Recycle(this.components);
                this.components = null;
            }

            this.RemoveFromComponentsDB(component);
        }

        public K GetChild<K>(long id) where K: Entity
        {
            if (this.children == null)
            {
                return null;
            }
            this.children.TryGetValue(id, out Entity child);
            return child as K;
        }

        public bool TryGetChild<K>(long id, out K k) where K : Entity
        {
            k = GetChild<K>(id);
            if (k == null)
                return false;
            return true;
        }

        public void RemoveChild(long id, bool dispose = true)
        {
            if (this.children == null)
            {
                return;
            }

            if (!this.children.TryGetValue(id, out Entity child))
            {
                return;
            }

            this.children.Remove(id);

            if (dispose)
                child.Dispose();
        }

        public void RemoveComponent<K>() where K : Entity
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Type type = typeof (K);
            Entity c = this.GetComponent(type);
            if (c == null)
            {
                return;
            }

            this.RemoveFromComponents(c);
            c.Dispose();
        }

        public void RemoveComponent(Entity component)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Entity c = this.GetComponent(component.GetType());
            if (c == null)
            {
                return;
            }

            if (c.InstanceId != component.InstanceId)
            {
                return;
            }

            this.RemoveFromComponents(c);
            c.Dispose();
        }

        public void RemoveComponent(Type type)
        {
            if (this.IsDisposed)
            {
                return;
            }

            Entity c = this.GetComponent(type);
            if (c == null)
            {
                return;
            }

            RemoveFromComponents(c);
            c.Dispose();
        }

        public K GetComponent<K>() where K : Entity
        {
            if (this.components == null)
            {
                return null;
            }

            Entity component;
            if (!this.components.TryGetValue(typeof (K), out component))
            {
                return default;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponent)
            {
                EventSystem.Instance.GetComponent(this, component);
            }

            return (K) component;
        }

        public K GetOrAdd<K>() where K : Entity, IAwake, new()
        {
            var k = this.GetComponent<K>();
            if (k == null)
                k = this.AddComponent<K>();
            return k;
        }

        public Entity GetComponent(Type type)
        {
            if (this.components == null)
            {
                return null;
            }

            Entity component;
            if (!this.components.TryGetValue(type, out component))
            {
                return null;
            }
            
            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponent)
            {
                EventSystem.Instance.GetComponent(this, component);
            }

            return component;
        }
        
        private static Entity Create(Type type, bool isFromPool)
        {
            Entity component;
            if (isFromPool)
            {
                component = (Entity)ObjectPool.Instance.Fetch(type);
            }
            else
            {
                component = Activator.CreateInstance(type) as Entity;
            }
            component.IsFromPool = isFromPool;
            component.IsCreated = true;
            component.IsNew = true;
            component.Id = 0;
            return component;
        }

        public Entity AddComponent(Entity component)
        {
            Type type = component.GetType();
            if (this.IsComponent)
            {
                throw new Exception($"cant add component in an component: {type.FullName}");
            }
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }
            return component;
        }

        public Entity AddComponent(Type type, bool isFromPool = false)
        {
            if (this.IsComponent)
            {
                throw new Exception($"cant add component in an component: {type.FullName}");
            }
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component);
            
            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }
            return component;
        }

        public K AddComponent<K>(bool isFromPool = false) where K : Entity, IAwake, new()
        {
            Type type = typeof (K);
            if (this.IsComponent)
            {
                throw new Exception($"cant add component in an component: {type.FullName}");
            }
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component);
            
            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }
            return component as K;
        }

        public K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            Type type = typeof (K);
            if (this.IsComponent)
            {
                throw new Exception($"cant add component in an component: {type.FullName}");
            }
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component, p1);
            
            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }
            return component as K;
        }

        public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            Type type = typeof (K);
            if (this.IsComponent)
            {
                throw new Exception($"cant add component in an component: {type.FullName}");
            }
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component, p1, p2);
            
            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }
            return component as K;
        }

        public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            Type type = typeof (K);
            if (this.IsComponent)
            {
                throw new Exception($"cant add component in an component: {type.FullName}");
            }
            if (this.components != null && this.components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EventSystem.Instance.Awake(component, p1, p2, p3);
            
            if (this is IAddComponent)
            {
                EventSystem.Instance.AddComponent(this, component);
            }
            return component as K;
        }
        
        public Entity AddChild(Entity entity)
        {
            entity.Parent = this;
            return entity;
        }

        public T AddChild<T>(bool isFromPool = false) where T : Entity, IAwake
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId(Options.Instance.Process);
            component.Parent = this;

            EventSystem.Instance.Awake(component);
            return component;
        }

        public T AddChild<T, A>(A a, bool isFromPool = false) where T : Entity, IAwake<A>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId(Options.Instance.Process);
            component.Parent = this;

            EventSystem.Instance.Awake(component, a);
            return component;
        }

        public T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId(Options.Instance.Process);
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId(Options.Instance.Process);
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b, c);
            return component;
        }

        public T AddChild<T, A, B, C, D>(A a, B b, C c, D d, bool isFromPool = false) where T : Entity, IAwake<A, B, C, D>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId(Options.Instance.Process);
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b, c, d);
            return component;
        }

        public T AddChildWithId<T>(long id, bool isFromPool = false) where T : Entity, IAwake, new()
        {
            Type type = typeof (T);
            T component = Entity.Create(type, isFromPool) as T;
            component.Id = id;
            component.Parent = this;
            EventSystem.Instance.Awake(component);
            return component;
        }

        public T AddChildWithId<T, A>(long id, A a, bool isFromPool = false) where T : Entity, IAwake<A>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EventSystem.Instance.Awake(component, a);
            return component;
        }

        public T AddChildWithId<T, A, B>(long id, A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChildWithId<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
        {
            Type type = typeof (T);
            T component = (T) Entity.Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EventSystem.Instance.Awake(component, a, b, c);
            return component;
        }
    }
}