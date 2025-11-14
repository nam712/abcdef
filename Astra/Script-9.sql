

-- ##################################################################
-- ## PHẦN 1: TẠO CÁC BẢNG CƠ BẢN (KHÔNG PHỤ THUỘC/PHỤ THUỘC SHOP_OWNER)
-- ##################################################################

-- Bảng nhà cung cấp (Phụ thuộc shop_owner - sẽ thêm FK sau)
CREATE TABLE suppliers (
    supplier_id SERIAL PRIMARY KEY,                             -- Mã định danh duy nhất của nhà cung cấp
    shop_owner_id INT NOT NULL,                                -- 🔒 Chủ shop sở hữu nhà cung cấp này
    supplier_code VARCHAR(50) NOT NULL,                        -- Mã nhà cung cấp do người dùng tạo
    supplier_name VARCHAR(255) NOT NULL,                       -- Tên nhà cung cấp
    contact_person VARCHAR(255),                               -- Người liên hệ (NULL)
    phone VARCHAR(20),                                          -- Số điện thoại (NULL)
    email VARCHAR(100),                                         -- Email (NULL)
    address VARCHAR(255),                                       -- Địa chỉ (NULL)
    tax_code VARCHAR(50),                                       -- Mã số thuế (NULL)
    bank_account VARCHAR(100),                                  -- Số tài khoản ngân hàng (NULL)
    bank_name VARCHAR(255),                                     -- Tên ngân hàng (NULL)
    price_list VARCHAR(255),                                    -- Bảng giá/ghi chú giá (NULL)
    logo_url  VARCHAR(255),                                     -- Đường dẫn logo nhà cung cấp (NULL)
    status VARCHAR(20) NOT NULL DEFAULT 'active',              -- Trạng thái (active/inactive)
    notes TEXT,                                                 -- Ghi chú thêm (NULL)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    
    -- 🔒 Multi-tenancy: Mỗi shop owner chỉ thấy nhà cung cấp của mình
    CONSTRAINT uq_suppliers_code_owner UNIQUE (supplier_code, shop_owner_id)
);


-- Bảng ngành hàng (Không phụ thuộc)
CREATE TABLE business_categories (
    category_id SERIAL PRIMARY KEY,                             -- Mã định danh duy nhất của ngành hàng
    category_name VARCHAR(255) NOT NULL UNIQUE                 -- Tên ngành hàng
);

-- Bảng loại sản phẩm (Tự tham chiếu, có thể tạo trước)
CREATE TABLE product_categories (
    category_id SERIAL PRIMARY KEY,                             -- Mã định danh duy nhất của danh mục
    category_name VARCHAR(255) NOT NULL UNIQUE,                 -- Tên danh mục sản phẩm
    description TEXT,                                           -- Mô tả chi tiết về danh mục (NULL)
    parent_category_id INT,                                     -- Mã danh mục cha (để tạo cấu trúc đa cấp)
    status VARCHAR(20) NOT NULL DEFAULT 'active',              -- Trạng thái (active/inactive)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo bản ghi
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    
    CONSTRAINT fk_prod_cat_parent FOREIGN KEY (parent_category_id) REFERENCES product_categories(category_id) ON DELETE SET NULL
);

-- Bảng quản lý các phương thức thanh toán (Không phụ thuộc)
CREATE TABLE payment_methods (
    payment_method_id SERIAL PRIMARY KEY,                         -- Mã định danh duy nhất
    method_name VARCHAR(100) NOT NULL UNIQUE,                     -- Tên hiển thị cho người dùng (VD: "Tiền mặt", "Ví MoMo")
    method_code VARCHAR(50) NOT NULL UNIQUE,                      -- Mã để xử lý trong hệ thống (VD: "cash", "momo")
    description TEXT,                                             -- Mô tả chi tiết (NULL)
    is_active BOOLEAN NOT NULL DEFAULT TRUE,                      -- Trạng thái kích hoạt (True/False)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,      -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP       -- Ngày cập nhật cuối
);

-- Bảng khách hàng (Phụ thuộc shop_owner - sẽ thêm FK sau)
CREATE TABLE customers (
    customer_id SERIAL PRIMARY KEY,                             -- Mã định danh duy nhất của khách hàng
    shop_owner_id INT NOT NULL,                                -- 🔒 Chủ shop sở hữu khách hàng này
    customer_code VARCHAR(50) NOT NULL,                        -- Mã khách hàng do người dùng tạo
    customer_name VARCHAR(255) NOT NULL,                       -- Tên khách hàng
    phone VARCHAR(20),                                          -- Số điện thoại (NULL)
    email VARCHAR(100),                                         -- Email (NULL)
    address VARCHAR(255),                                       -- Địa chỉ (NULL)
    tax_code VARCHAR(50),                                       -- Mã số thuế (khách hàng là doanh nghiệp) (NULL)
    customer_type VARCHAR(20) NOT NULL DEFAULT 'retail',       -- Loại khách hàng (retail/wholesale/corporate)
    date_of_birth DATE,                                         -- Ngày sinh (NULL)
    gender VARCHAR(10),                                         -- Giới tính (Male/Female/Other) (NULL)
    id_card VARCHAR(20),                                        -- Số CMND/CCCD (NULL)
    bank_account VARCHAR(100),                                  -- Số tài khoản ngân hàng (NULL)
    bank_name VARCHAR(255),                                     -- Tên ngân hàng (NULL)
    total_debt DECIMAL(18, 2) DEFAULT 0,                       -- Tổng tiền nợ (NULL)
    total_purchase_amount DECIMAL(18, 2) DEFAULT 0,            -- Tổng tiền đã mua
    total_purchase_count INT DEFAULT 0,                        -- Tổng số đơn đã mua
    loyalty_points INT DEFAULT 0,                              -- Điểm tích lũy (NULL)
    segment VARCHAR(50),                                        -- Phân loại khách hàng (VIP, Thường xuyên, v.v.) (NULL)
    source VARCHAR(100),                                        -- Nguồn khách hàng (Facebook, Google, Giới thiệu, v.v.) (NULL)
    avatar_url VARCHAR(255),                                   -- Đường dẫn ảnh đại diện khách hàng (NULL)
    status VARCHAR(20) NOT NULL DEFAULT 'active',              -- Trạng thái (active/inactive/blocked)
    notes TEXT,                                                 -- Ghi chú thêm (NULL)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    
    -- 🔒 Multi-tenancy: Mỗi shop owner chỉ thấy khách hàng của mình
    CONSTRAINT uq_customers_code_owner UNIQUE (customer_code, shop_owner_id)
);


-- Bảng nhân viên (Phụ thuộc shop_owner - sẽ thêm FK sau)
CREATE TABLE employees (
    employee_id SERIAL PRIMARY KEY,                             -- Mã định danh duy nhất của nhân viên
    shop_owner_id INT NOT NULL,                                -- 🔒 Chủ shop sở hữu nhân viên này
    employee_code VARCHAR(50) NOT NULL,                        -- Mã nhân viên do người dùng tạo
    employee_name VARCHAR(255) NOT NULL,                       -- Tên nhân viên
    phone VARCHAR(20),                                          -- Số điện thoại (NULL)
    email VARCHAR(100),                                         -- Email (NULL)
    address VARCHAR(255),                                       -- Địa chỉ (NULL)
    date_of_birth DATE,                                         -- Ngày sinh (NULL)
    gender VARCHAR(10),                                         -- Giới tính (Male/Female/Other) (NULL)
    id_card VARCHAR(20),                                        -- Số CMND/CCCD (NULL)
    position VARCHAR(100),                                      -- Chức vụ (Bán hàng, Quản lý, v.v.) (NULL)
    department VARCHAR(100),                                    -- Phòng ban (NULL)
    hire_date DATE NOT NULL,                                   -- Ngày vào làm
    salary DECIMAL(18, 2),                                      -- Mức lương (NULL)
    salary_type VARCHAR(20),                                    -- Loại lương (monthly/hourly/commission) (NULL)
    bank_account VARCHAR(100),                                  -- Số tài khoản ngân hàng (NULL)
    bank_name VARCHAR(255),                                     -- Tên ngân hàng (NULL)
    username VARCHAR(100) UNIQUE,                              -- Tên tài khoản đăng nhập (NULL)
    password VARCHAR(255),                                      -- Mật khẩu (mã hóa) (NULL)
    permissions VARCHAR(255),                                   -- Danh sách quyền hạn (JSON format) (NULL)
    avatar_url VARCHAR(255),                                    -- Đường dẫn ảnh đại diện nhân viên (NULL)
    work_status VARCHAR(20) NOT NULL DEFAULT 'active',         -- Trạng thái làm việc (active/inactive/resigned/on_leave)
    notes TEXT,                                                 -- Ghi chú thêm (NULL)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,   -- Ngày cập nhật cuối
    
    -- 🔒 Multi-tenancy: Mỗi shop owner chỉ thấy nhân viên của mình
    CONSTRAINT uq_employees_code_owner UNIQUE (employee_code, shop_owner_id),
    
    -- Quy tắc: Mật khẩu là bắt buộc nếu có tên đăng nhập
    CONSTRAINT check_password_if_username_exists CHECK ( (username IS NULL) OR (password IS NOT NULL) )
);

-- Bảng khuyến mãi
CREATE TABLE promotions (
    promotion_id SERIAL PRIMARY KEY,
    promotion_code VARCHAR(50) NOT NULL,
    promotion_name VARCHAR(200) NOT NULL,
    description TEXT,
    promotion_type VARCHAR(50) NOT NULL,  -- percentage, fixed, buy_x_get_y, free_shipping
    discount_value NUMERIC(18,2) NOT NULL,
    min_purchase_amount NUMERIC(18,2),
    max_discount_amount NUMERIC(18,2),
    start_date TIMESTAMP NOT NULL,
    end_date TIMESTAMP NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'active',  -- active, inactive, expired
    usage_limit INT,
    usage_count INT NOT NULL DEFAULT 0,
    applicable_products JSONB,   -- JSON array of product IDs
    applicable_customers JSONB,  -- JSON array of customer IDs
    shop_owner_id INT NOT NULL,  -- chỉ là cột dữ liệu, không FK
    invoice_id INT,              -- Mã hóa đơn áp dụng khuyến mãi (NULL - sẽ thêm FK sau khi tạo bảng invoices)
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_promotions_code_shopowner UNIQUE (promotion_code, shop_owner_id)
);
-- ##################################################################
-- ## PHẦN 2: TẠO CÁC BẢNG PHỤ THUỘC (BẬC 1)
-- ##################################################################

-- Bảng chủ shop (Phụ thuộc: business_categories)
CREATE TABLE shop_owner (
    shop_owner_id SERIAL PRIMARY KEY,                           -- Mã định danh duy nhất của chủ shop
    shop_owner_name VARCHAR(255) NOT NULL,                     -- Tên chủ shop
    phone VARCHAR(20) NOT NULL UNIQUE,                         -- Số điện thoại (login)
    email VARCHAR(100),                                         -- Email (NULL)
    address  VARCHAR(255),                                      -- Địa chỉ cá nhân (NULL)
    date_of_birth DATE,                                         -- Ngày sinh (NULL)
    gender VARCHAR(10),                                         -- Giới tính (Male/Female/Other) (NULL)
    id_card_number VARCHAR(20),                                 -- Số CCCD (NULL)
    id_card_issued_place VARCHAR(255),                          -- Nơi cấp CCCD (NULL)
    id_card_issued_date DATE,                                   -- Ngày cấp CCCD (NULL)
    tax_code VARCHAR(50),                                       -- Mã số thuế (NULL)
    business_license_number VARCHAR(50),                        -- Số giấy phép kinh doanh (NULL)
    business_license_issued_date DATE,                          -- Ngày cấp GPKD (NULL)
    business_license_issued_place VARCHAR(255),                 -- Nơi cấp GPKD (NULL)
    password VARCHAR(255) NOT NULL,                            -- Mật khẩu (mã hóa)
    avatar_url VARCHAR(255),                                    -- Đường dẫn ảnh đại diện chủ shop (NULL)
    terms_and_conditions_agreed BOOLEAN NOT NULL DEFAULT FALSE, -- Đồng ý điều khoản & dịch vụ
    status VARCHAR(20) NOT NULL DEFAULT 'active',              -- Trạng thái (active/inactive)
    notes TEXT,                                                 -- Ghi chú thêm (NULL)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP  -- Ngày cập nhật cuối
);


-- Bảng sản phẩm (Phụ thuộc: product_categories, shop_owner)
CREATE TABLE products (
    product_id SERIAL PRIMARY KEY,                              -- Mã định danh duy nhất của sản phẩm
    shop_owner_id INT NOT NULL,                                -- 🔒 Sản phẩm thuộc chủ shop nào
    product_code VARCHAR(50) NOT NULL,                         -- Mã sản phẩm do người dùng tạo
    product_name VARCHAR(255) NOT NULL,                        -- Tên sản phẩm
    description VARCHAR(255),                                   -- Mô tả chi tiết sản phẩm (NULL)
    category_id INT,                                            -- Mã danh mục sản phẩm (NULL)
    brand VARCHAR(100),                                         -- Thương hiệu sản phẩm (NULL)
    supplier_name VARCHAR(255),                                -- Tên nhà cung cấp (ghi nhớ từ lần nhập gần nhất)
    price DECIMAL(18, 2) NOT NULL,                             -- Giá bán lẻ hiện tại
    cost_price DECIMAL(18, 2),                                 -- Giá vốn/giá nhập (NULL)
    stock INT NOT NULL DEFAULT 0,                              -- Số lượng tồn kho hiện tại
    min_stock INT DEFAULT 0,                                   -- Số lượng tối thiểu để cảnh báo (NULL)
    sku VARCHAR(100),                                          -- Mã SKU (NULL)
    barcode VARCHAR(100),                                      -- Mã vạch (NULL)
    unit VARCHAR(50),                                           -- Đơn vị tính (cái, bộ, hộp, kg...) (NULL)
    image_url VARCHAR(255),                                    -- Đường dẫn ảnh sản phẩm (NULL)
    status VARCHAR(20) NOT NULL DEFAULT 'active',              -- Trạng thái (active/inactive)
    weight DECIMAL(10, 2),                                     -- Cân nặng (kg) (NULL)
    dimension VARCHAR(100),                                    -- Kích thước (dài x rộng x cao) (NULL)
    notes TEXT,                                                 -- Ghi chú thêm (NULL)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    
    -- 🔒 Multi-tenancy: Mỗi shop owner có sản phẩm riêng
    CONSTRAINT uq_products_code_owner UNIQUE (product_code, shop_owner_id),
    CONSTRAINT fk_products_categories FOREIGN KEY (category_id) REFERENCES product_categories(category_id) ON DELETE SET NULL
);

-- Bảng phiếu nhập hàng từ nhà cung cấp (Phụ thuộc: suppliers)
CREATE TABLE purchase_orders (
    purchase_order_id SERIAL PRIMARY KEY,                       -- Mã định danh duy nhất của phiếu nhập
    shop_owner_id INT NOT NULL,                                --  Phiếu nhập thuộc chủ shop nào
    po_code VARCHAR(50) NOT NULL,                              -- Mã phiếu nhập do người dùng tạo
    supplier_id INT NOT NULL,                                  -- Mã nhà cung cấp (khóa ngoại)
    po_date TIMESTAMP NOT NULL,                                -- Ngày lập phiếu
    expected_delivery_date DATE,                               -- Ngày dự kiến nhận hàng (NULL)
    actual_delivery_date DATE,                                 -- Ngày thực tế nhận hàng (NULL)
    total_amount DECIMAL(18, 2) DEFAULT 0,                     -- Tổng giá trị của cả phiếu (sẽ được tính toán)
    status VARCHAR(20) NOT NULL DEFAULT 'pending',             -- Trạng thái của cả phiếu (pending/received/cancelled)
    payment_status VARCHAR(20) DEFAULT 'unpaid',               -- Trạng thái thanh toán của cả phiếu (unpaid/partial/paid)
    notes TEXT,                                                 -- Ghi chú chung cho cả phiếu
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    
    -- 🔒 Multi-tenancy: Mỗi shop owner có mã phiếu nhập riêng
    CONSTRAINT uq_po_code_owner UNIQUE (po_code, shop_owner_id),
    CONSTRAINT fk_po_suppliers FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id) ON DELETE RESTRICT
);

-- Bảng thông tin MoMo (Phụ thuộc: payment_methods)
CREATE TABLE momoinfos (
    id SERIAL PRIMARY KEY,
    order_id VARCHAR(255) NOT NULL,                               -- Mã đơn hàng MoMo
    order_info TEXT NOT NULL,                                     -- Nội dung thanh toán
    full_name VARCHAR(255),                                       -- Tên người thanh toán
    amount DECIMAL(18,2),                                         -- Số tiền
    date_paid TIMESTAMP,                                          -- Ngày thanh toán

    -- 🔗 Khóa ngoại trỏ đến bảng payment_methods
    payment_method_id INT NOT NULL,
    CONSTRAINT fk_momo_payment_method
        FOREIGN KEY (payment_method_id)
        REFERENCES payment_methods(payment_method_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

-- Bảng hóa đơn (Phụ thuộc: customers, employees, payment_methods)
CREATE TABLE invoices (
    invoice_id SERIAL PRIMARY KEY,                              -- Mã định danh duy nhất của hóa đơn
    invoice_code VARCHAR(50) NOT NULL UNIQUE,                  -- Mã hóa đơn do người dùng tạo
    customer_id INT,                                           -- Mã khách hàng (khóa ngoại) - nullable (khách lẻ)
    employee_id INT NOT NULL,                                  -- Mã nhân viên lập hóa đơn (khóa ngoại) - REQUIRED
    shop_id INT,                                                -- Mã chi nhánh (khóa ngoại) (NULL - sẽ được thêm FK sau)
    invoice_date TIMESTAMP NOT NULL,                           -- Ngày lập hóa đơn
    total_amount DECIMAL(18, 2) NOT NULL,                      -- Tổng tiền trước giảm giá
    discount_amount DECIMAL(18, 2) DEFAULT 0,                  -- Tiền giảm giá (NULL)
    final_amount DECIMAL(18, 2) NOT NULL,                      -- Tổng tiền cuối cùng (total_amount - discount_amount)
    amount_paid DECIMAL(18, 2) DEFAULT 0,                      -- Tiền đã thanh toán
    payment_method_id INT,
    payment_status VARCHAR(20) NOT NULL DEFAULT 'unpaid',      -- Trạng thái thanh toán (unpaid/partial/paid)
    notes TEXT,                                                 -- Ghi chú thêm (NULL)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    CONSTRAINT fk_invoices_customers FOREIGN KEY (customer_id) REFERENCES customers(customer_id) ON DELETE SET NULL,
    CONSTRAINT fk_invoices_employees FOREIGN KEY (employee_id) REFERENCES employees(employee_id) ON DELETE RESTRICT,
    CONSTRAINT fk_invoices_payment_methods FOREIGN KEY (payment_method_id) REFERENCES payment_methods(payment_method_id) ON DELETE SET NULL
);


-- ##################################################################
-- ## PHẦN 3: TẠO CÁC BẢNG CHI TIẾT (PHỤ THUỘC BẬC 2)
-- ##################################################################

--Bảng chi tiết phiếu sản phẩm (Phụ thuộc: purchase_orders, products)
CREATE TABLE purchase_order_details (
    purchase_order_detail_id SERIAL PRIMARY KEY,                -- Mã định danh duy nhất của dòng chi tiết
    purchase_order_id INT NOT NULL,                            -- Mã phiếu nhập (liên kết với bảng purchase_orders)
    product_id INT NOT NULL,                                   -- Mã sản phẩm (khóa ngoại)
    quantity INT NOT NULL,                                     -- Số lượng nhập
    import_price DECIMAL(18, 2) NOT NULL,                      -- Giá nhập của lô này
    final_amount DECIMAL(18, 2) NOT NULL,                      -- Tổng tiền cuối cùng (quantity * import_price)

    CONSTRAINT fk_pod_orders FOREIGN KEY (purchase_order_id) REFERENCES purchase_orders(purchase_order_id) ON DELETE CASCADE,
    CONSTRAINT fk_pod_products FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
);

-- Bảng chi tiết hóa đơn (Phụ thuộc: invoices, products)
CREATE TABLE invoice_details (
    invoice_detail_id SERIAL PRIMARY KEY,                       -- Mã định danh duy nhất của chi tiết hóa đơn
    invoice_id INT NOT NULL,                                   -- Mã hóa đơn (khóa ngoại)
    product_id INT NOT NULL,                                   -- Mã sản phẩm (khóa ngoại)
    quantity INT NOT NULL,                                     -- Số lượng bán
    unit_price DECIMAL(18, 2) NOT NULL,                        -- Giá bán lúc lập hóa đơn
    line_total DECIMAL(18, 2) NOT NULL,                        -- Thành tiền (quantity * unit_price)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày tạo
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Ngày cập nhật cuối
    CONSTRAINT fk_id_invoices FOREIGN KEY (invoice_id) REFERENCES invoices(invoice_id) ON DELETE CASCADE,
    CONSTRAINT fk_id_products FOREIGN KEY (product_id) REFERENCES products(product_id) ON DELETE RESTRICT
);


-- ##################################################################
-- ## PHẦN 4: CHÈN DỮ LIỆU BAN ĐẦU
-- ##################################################################

INSERT INTO payment_methods (method_name, method_code, description)
VALUES 
('Thanh toán MoMo', 'momo', 'Ví điện tử MoMo'),
('Tiền mặt', 'cash', 'Thanh toán khi nhận hàng'),
('Chuyển khoản ngân hàng', 'bank_transfer', 'Khách tự chuyển khoản');




-- ##################################################################
-- ## MIGRATION: MULTI-STORE MODEL (1 Owner → Nhiều Shops)
-- ## Chiến lược: ISOLATED (Mỗi shop độc lập hoàn toàn)
-- ##################################################################

-- ========== BƯỚC 1: TẠO BẢNG SHOPS (Chi nhánh) ==========
CREATE TABLE shops (
    shop_id SERIAL PRIMARY KEY,
    shop_owner_id INT NOT NULL,                    -- Chủ shop sở hữu
    shop_code VARCHAR(50) NOT NULL UNIQUE,         -- Mã cửa hàng (VD: CN-HN, CN-HCM)
    shop_name VARCHAR(255) NOT NULL,               -- Tên chi nhánh
    shop_address VARCHAR(255),                     -- Địa chỉ chi nhánh
    shop_phone VARCHAR(20),                        -- SĐT chi nhánh
    shop_email VARCHAR(100),                       -- Email chi nhánh
    manager_name VARCHAR(255),                     -- Tên quản lý chi nhánh
    manager_phone VARCHAR(20),                     -- SĐT quản lý
    business_category_id INT,                      -- Mã ngành hàng (khóa ngoại)
    status VARCHAR(20) NOT NULL DEFAULT 'active',  -- active/inactive/closed
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_shops_owner 
    FOREIGN KEY (shop_owner_id) REFERENCES shop_owner(shop_owner_id) ON DELETE CASCADE,
    CONSTRAINT fk_shop_owner_category FOREIGN KEY (business_category_id) REFERENCES business_categories(category_id) ON DELETE SET NULL
);

-- ========== BƯỚC 3: THÊM FOREIGN KEY CHỈ CHO INVOICES (CHỈ INVOICE GIỮ SHOP_ID) ==========
-- Thêm ràng buộc khóa ngoại cho invoices.shop_id
ALTER TABLE invoices
ADD CONSTRAINT fk_invoices_shops 
FOREIGN KEY (shop_id) REFERENCES shops(shop_id) ON DELETE SET NULL;

-- Indexes cho shops
CREATE INDEX idx_shops_owner_id ON shops(shop_owner_id);
CREATE INDEX idx_shops_status ON shops(status);
CREATE INDEX idx_shops_code ON shops(shop_code);

-- Indexes cho promotions
CREATE INDEX idx_promotions_invoice_id ON promotions(invoice_id);
CREATE INDEX idx_promotions_shop_owner_id ON promotions(shop_owner_id);
CREATE INDEX idx_promotions_status ON promotions(status);
CREATE INDEX idx_promotions_start_date ON promotions(start_date);
CREATE INDEX idx_promotions_end_date ON promotions(end_date);

-- Thêm ràng buộc khóa ngoại cho promotions.invoice_id (SAU KHI ĐÃ TẠO BẢNG INVOICES)
ALTER TABLE promotions
ADD CONSTRAINT fk_promotions_invoices
FOREIGN KEY (invoice_id) REFERENCES invoices(invoice_id) ON DELETE SET NULL;


-- ##################################################################
-- ## PHẦN 5: TẠO CHỈ MỤC (INDEXES)
-- ##################################################################

-- Index cho bảng shop_owner
CREATE INDEX idx_shop_owner_email ON shop_owner(email);
CREATE INDEX idx_shop_owner_status ON shop_owner(status);


-- Index cho bảng suppliers
CREATE INDEX idx_suppliers_shop_owner_id ON suppliers(shop_owner_id);  -- 🔒 Multi-tenancy
CREATE INDEX idx_suppliers_name ON suppliers(supplier_name);
CREATE INDEX idx_suppliers_status ON suppliers(status);
CREATE INDEX idx_suppliers_phone ON suppliers(phone);

-- Index cho bảng products
CREATE INDEX idx_products_shop_owner_id ON products(shop_owner_id);  -- 🔒 Multi-tenancy
CREATE INDEX idx_products_name ON products(product_name);
CREATE INDEX idx_products_status ON products(status);
CREATE INDEX idx_products_category_id ON products(category_id);
CREATE INDEX idx_products_brand ON products(brand);
CREATE INDEX idx_products_supplier_name ON products(supplier_name);  -- Index cho tìm kiếm theo tên NCC

-- Index cho bảng purchase_orders
CREATE INDEX idx_po_supplier_id ON purchase_orders(supplier_id);
CREATE INDEX idx_po_shop_owner_id ON purchase_orders(shop_owner_id);  -- 🔒 Multi-tenancy
CREATE INDEX idx_pod_order_id ON purchase_order_details(purchase_order_id); 
CREATE INDEX idx_pod_product_id ON purchase_order_details(product_id);   
CREATE INDEX idx_po_status ON purchase_orders(status);
CREATE INDEX idx_po_payment_status ON purchase_orders(payment_status);
CREATE INDEX idx_po_date ON purchase_orders(po_date);

-- Index cho bảng customers
CREATE INDEX idx_customers_shop_owner_id ON customers(shop_owner_id);  -- 🔒 Multi-tenancy
CREATE INDEX idx_customers_name ON customers(customer_name);
CREATE INDEX idx_customers_phone ON customers(phone);
CREATE INDEX idx_customers_email ON customers(email);
CREATE INDEX idx_customers_status ON customers(status);
CREATE INDEX idx_customers_customer_type ON customers(customer_type);
CREATE INDEX idx_customers_gender ON customers(gender);
CREATE INDEX idx_customers_date_of_birth ON customers(date_of_birth);
CREATE INDEX idx_customers_segment ON customers(segment);
CREATE INDEX idx_customers_total_purchase_amount ON customers(total_purchase_amount);

-- Index cho bảng invoices
CREATE INDEX idx_invoices_customer_id ON invoices(customer_id);
CREATE INDEX idx_invoices_employee_id ON invoices(employee_id);
CREATE INDEX idx_invoices_shop_id ON invoices(shop_id);
CREATE INDEX idx_invoices_invoice_date ON invoices(invoice_date);
CREATE INDEX idx_invoices_payment_status ON invoices(payment_status);
CREATE INDEX idx_invoices_payment_method_id ON invoices(payment_method_id); -- Đã thêm vào đây

-- Index cho bảng invoice_details
CREATE INDEX idx_invoice_details_invoice_id ON invoice_details(invoice_id);
CREATE INDEX idx_invoice_details_product_id ON invoice_details(product_id);

-- Index cho bảng payment_methods
CREATE INDEX idx_payment_methods_code ON payment_methods(method_code);
CREATE INDEX idx_payment_methods_active ON payment_methods(is_active);

-- Index cho bảng employees
CREATE INDEX idx_employees_shop_owner_id ON employees(shop_owner_id);  -- 🔒 Multi-tenancy
CREATE INDEX idx_employees_name ON employees(employee_name);
CREATE INDEX idx_employees_phone ON employees(phone);
CREATE INDEX idx_employees_email ON employees(email);
CREATE INDEX idx_employees_position ON employees(position);
CREATE INDEX idx_employees_department ON employees(department);
CREATE INDEX idx_employees_work_status ON employees(work_status);
CREATE INDEX idx_employees_gender ON employees(gender);
CREATE INDEX idx_employees_date_of_birth ON employees(date_of_birth);
CREATE INDEX idx_employees_hire_date ON employees(hire_date);