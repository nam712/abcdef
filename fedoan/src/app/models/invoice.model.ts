export interface Invoice {
  invoiceId?: number;
  invoiceCode: string;
  customerId: number;
  employeeId?: number | null;
  invoiceDate: Date | string;
  totalAmount: number;
  discountAmount?: number;
  finalAmount: number;
  amountPaid?: number;
  paymentMethodId?: number | null;
  paymentStatus: string;
  notes?: string;
  createdAt?: Date | string;
  updatedAt?: Date | string;
  
  // Navigation properties
  customerName?: string;
  employeeName?: string;
  paymentMethodName?: string;
  invoiceDetails?: InvoiceDetail[];
}

export interface InvoiceDetail {
  invoiceDetailId?: number;
  invoiceId?: number;
  productId: number;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  productName?: string;
  productCode?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T | any;
  errors?: string[];
}
