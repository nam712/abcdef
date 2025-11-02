export interface CreatePurchaseOrderDetailDto {
  productId: number;
  quantity: number;
  importPrice: number;
}

export interface CreatePurchaseOrderDto {
  poCode: string;
  supplierId: number;
  poDate: string; // ISO string
  expectedDeliveryDate?: string | null;
  notes?: string;
  details: CreatePurchaseOrderDetailDto[];
}
