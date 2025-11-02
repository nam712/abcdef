export interface Manufacturer {
  id: number;
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  website?: string;
  description?: string;
  isActive: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface CreateManufacturerDto {
  name: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  address?: string;
  website?: string;
  description?: string;
}

export interface UpdateManufacturerDto extends CreateManufacturerDto {
  isActive?: boolean;
}

export interface SearchManufacturerDto {
  keyword?: string;
  page?: number;
  pageSize?: number;
}

export interface ManufacturerResponse {
  success: boolean;
  message: string;
  data?: any;
  errors?: string[];
}
