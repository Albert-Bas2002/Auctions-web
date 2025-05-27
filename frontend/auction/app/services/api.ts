interface ErrorDto {
  error: string;
  details: string;
  status: number;
}

interface ApiResponse<T> {
  data?: T | null;
  error?: ErrorDto | null;
}
interface ContactDto {
  contactId: string;
}
interface AuctionListItem {
  auctionId: string;
  creationTime: string;
  endTime: string;
  title: string;
  reserve: number;
  currentBid: number;
}
export interface LoginRequest {
  email: string;
  password: string;
}
export interface AuctionCreateRequest {
  title: string;
  description: string;
  auctionDurationInDays: number;
  reserve: number;
}

export interface AuctionPageDto {
  auctionId: string;
  creationTime: string;
  endTime: string;
  title: string;
  description: string;
  reserve: number;
  currentBid: number;
  type: string;
  biddersBid?: number | null;
  status: string;
}

export interface ChangeEmailRequest {
  newEmail: string;
}

export interface ChangeContactsRequest {
  newContacts: string;
}
export interface ChangePasswordRequest {
  newPassword: string;
  previousPassword: string;
}
export interface ChangeUserNameRequest {
  newUserName: string;
}
export interface UserInfoResponse {
  userName: string;
  email: string;
  contacts: string;
}
export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  contacts: string;
}
const BASE_URL = "http://localhost:5003";

export const Login = async (
  email: string,
  password: string
): Promise<{ success: boolean; message?: string }> => {
  const loginRequest: LoginRequest = { email, password };

  try {
    const response = await fetch(`${BASE_URL}/User/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(loginRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true, message: "Successful entry" };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const Register = async (
  userName: string,
  email: string,
  password: string,
  contacts: string
): Promise<{ success: boolean; message?: string }> => {
  const registerRequest: RegisterRequest = {
    userName,
    email,
    password,
    contacts,
  };

  try {
    const response = await fetch(`${BASE_URL}/User/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(registerRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true, message: "Successful registration" };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const GetUserInfo = async (
  userId: string
): Promise<{
  success: boolean;
  message?: string;
  userName?: string;
  email?: string;
  contacts?: string;
}> => {
  try {
    const response = await fetch(`${BASE_URL}/User/user-info/${userId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
    });

    const json: ApiResponse<UserInfoResponse> = await response.json();

    if (response.ok && json.data) {
      return {
        success: true,
        email: json.data.email,
        userName: json.data.userName,
        contacts: json.data.contacts,
      };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const GetAuctionPhoto = async (
  auctionId: string,
  index: number
): Promise<{
  success: boolean;
  imageUrl?: string;
  message?: string;
}> => {
  try {
    const response = await fetch(
      `${BASE_URL}/Auction/photo/auction/${auctionId}/index/${index}`,
      {
        method: "GET",
        credentials: "include",
      }
    );
    if (!response.ok) {
      return {
        success: false,
        message: `Ошибка сервера: ${response.status}`,
      };
    }

    const blob = await response.blob();
    const imageUrl = URL.createObjectURL(blob);

    return {
      success: true,
      imageUrl,
    };
  } catch (error: any) {
    return {
      success: false,
      message: error.message || "Unknown server error",
    };
  }
};

export const ChangeEmail = async (
  newEmail: string
): Promise<{ success: boolean; message?: string }> => {
  const changeEmailRequest: ChangeEmailRequest = { newEmail };
  try {
    const response = await fetch(`${BASE_URL}/User/change-email`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(changeEmailRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true, message: "Successful email change" };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const ChangeContacts = async (
  newContacts: string
): Promise<{ success: boolean; message?: string }> => {
  const changeContactsRequest: ChangeContactsRequest = { newContacts };
  try {
    const response = await fetch(`${BASE_URL}/User/change-contacts`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(changeContactsRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true, message: "Successful contacts change" };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const ChangeUserName = async (
  newUserName: string
): Promise<{ success: boolean; message?: string }> => {
  const changeUserNameRequest: ChangeUserNameRequest = { newUserName };
  try {
    const response = await fetch(`${BASE_URL}/User/change-username`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(changeUserNameRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true, message: "Successful user name change" };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const ChangePassword = async (
  newPassword: string,
  previousPassword: string
): Promise<{ success: boolean; message?: string }> => {
  const changePasswordRequest: ChangePasswordRequest = {
    newPassword,
    previousPassword,
  };
  try {
    const response = await fetch(`${BASE_URL}/User/change-password`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(changePasswordRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true, message: "Successful password change" };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};
export const GetAuctions = async (
  page: number,
  sortType?: string
): Promise<{
  success: boolean;
  message?: string;
  auctionListItems?: AuctionListItem[];
}> => {
  try {
    const queryParams = new URLSearchParams();
    queryParams.append("page", page.toString());
    if (sortType) {
      queryParams.append("sortType", sortType);
    }

    const response = await fetch(
      `${BASE_URL}/Auction/auctions?${queryParams.toString()}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      }
    );

    const json: ApiResponse<AuctionListItem[]> = await response.json();

    if (response.ok && json.data) {
      return {
        success: true,
        auctionListItems: json.data,
      };
    }

    if ((json as any).error) {
      return {
        success: false,
        message: (json as any).error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};
export const GetAuctionsCategory = async (
  category: string,
  active?: boolean
): Promise<{
  success: boolean;
  message?: string;
  auctionListItems?: AuctionListItem[];
}> => {
  try {
    const queryParams = new URLSearchParams();
    queryParams.append("category", category);
    if (active !== undefined) {
      queryParams.append("active", String(active));
    }

    const response = await fetch(
      `${BASE_URL}/Auction/auctions/category?${queryParams.toString()}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      }
    );

    const json: ApiResponse<AuctionListItem[]> = await response.json();

    if (response.ok && json.data) {
      return {
        success: true,
        auctionListItems: json.data,
      };
    }

    if ((json as any).error) {
      return {
        success: false,
        message: (json as any).error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const GetAuctionPage = async (
  auctionId: string
): Promise<{
  success: boolean;
  auctionPage?: AuctionPageDto;
  message?: string;
}> => {
  try {
    const response = await fetch(`${BASE_URL}/Auction/auction/${auctionId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
    });

    const json: ApiResponse<AuctionPageDto> = await response.json();

    if (response.ok && json.data) {
      return {
        success: true,
        auctionPage: json.data,
      };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return {
      success: false,
      message: "Unknown server error",
    };
  } catch (error) {
    return {
      success: false,
      message: "Network or server error",
    };
  }
};
export const CloseAuction = async (
  auctionId?: string
): Promise<{
  success: boolean;
  message?: string;
}> => {
  try {
    if (auctionId == null) {
      return { success: false };
    }
    const response = await fetch(
      `${BASE_URL}/Auction/auction/close/${auctionId}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      }
    );

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};

export const CreateAuction = async (
  title: string,
  description: string,
  auctionDurationInDays: number,
  reserve: number
): Promise<{ success: boolean; message?: string }> => {
  const auctionCreateRequest: AuctionCreateRequest = {
    title,
    description,
    auctionDurationInDays,
    reserve,
  };

  try {
    const response = await fetch(`${BASE_URL}/Auction/auction/create`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(auctionCreateRequest),
      credentials: "include",
    });

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return {
        success: true,
        message: "The auction has been successfully set up",
      };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};
export const AddAuctionPhotos = async (
  files: FileList | File[],
  auctionId?: string
): Promise<{ success: boolean; message?: string }> => {
  try {
    if (auctionId == null) {
      return { success: false };
    }
    if (!files || (files instanceof Array && files.length === 0)) {
      return { success: false, message: "No files to download" };
    }
    const formData = new FormData();

    Array.from(files).forEach((file) => {
      formData.append("photos", file);
    });
    const response = await fetch(
      `${BASE_URL}/Auction/auction/${auctionId}/upload-photos`,
      {
        method: "POST",
        body: formData,
        credentials: "include",
      }
    );

    const json = await response.json();

    if (response.ok) {
      return { success: true };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details || "Server error",
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};
export const DeleteAuctionPhotos = async (
  indexes: number[],
  auctionId?: string
): Promise<{ success: boolean; message?: string }> => {
  try {
    if (!auctionId) {
      return { success: false, message: "No auction ID specified" };
    }

    if (!indexes || indexes.length === 0) {
      return { success: false, message: "No photos selected for deletion" };
    }

    const response = await fetch(
      `${BASE_URL}/Auction/auction/${auctionId}/delete-photos`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(indexes),
        credentials: "include",
      }
    );

    const json = await response.json();

    if (response.ok) {
      return { success: true };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details || "Error when deleting a photo",
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};
export const CompleteAuctionDeal = async (
  auctionId?: string
): Promise<{
  success: boolean;
  message?: string;
}> => {
  try {
    if (!auctionId) {
      return { success: false, message: "Auction ID not specified" };
    }

    const response = await fetch(
      `${BASE_URL}/Auction/auction/complete-deal/${auctionId}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      }
    );

    const json: ApiResponse<object> = await response.json();

    if (response.ok) {
      return { success: true };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details || "Error when completing a deal",
      };
    }

    return { success: false, message: "Unknown server error" };
  } catch (error) {
    return { success: false, message: "Network or server error" };
  }
};
export const GetCreatorOrWinnerId = async (
  auctionId: string
): Promise<{
  success: boolean;
  id?: string;
  message?: string;
}> => {
  try {
    const response = await fetch(
      `${BASE_URL}/Auction/auction/winner-creator-info/${auctionId}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      }
    );

    const json: ApiResponse<ContactDto> = await response.json();

    if (response.ok && json.data) {
      return {
        success: true,
        id: json.data.contactId,
      };
    }

    if (json.error) {
      return {
        success: false,
        message: json.error.details,
      };
    }

    return {
      success: false,
      message: "Unknown server error",
    };
  } catch (error) {
    return {
      success: false,
      message: "Network or server error",
    };
  }
};
